using System.Xml.Linq;

using AppEngine.Accounting.Account;
using AppEngine.Accounting.Bookings;
using AppEngine.Authorization;
using AppEngine.DataAccess;
using AppEngine.Mediator;
using AppEngine.ReadModels;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppEngine.Accounting.Iso20022.Camt;

public class SavePaymentFileCommand : IRequest, IPartitionBoundRequest, IReceiveFileCommand
{
    public Guid PartitionId { get; set; }
    public FileUpload File { get; set; } = null!;
}

public class SavePaymentFileCommandHandler(IRepository<PaymentsFile> paymentFiles,
                                           IRepository<Booking> bookings,
                                           ILogger<SavePaymentFileCommandHandler> log,
                                           BankAccountConfiguration configuration,
                                           ChangeTrigger changeTrigger)
    : IRequestHandler<SavePaymentFileCommand>
{
    public async Task Handle(SavePaymentFileCommand command, CancellationToken cancellationToken)
    {
        command.File.FileStream.Position = 0;

        switch (command.File.ContentType)
        {
            case "text/xml":
            case "application/xml":
                await SaveCamt(command.PartitionId,
                               command.File.FileStream,
                               cancellationToken);

                break;

            case "image/jpeg":
            case "image/png":
                await TrySavePaymentSlipImage(command.PartitionId,
                                              command.File.FileStream,
                                              command.File.Filename,
                                              command.File.ContentType);

                break;

            case "application/x-gzip":
                await SaveCamtWithPaymentSlips(command.PartitionId,
                                               command.File.FileStream,
                                               cancellationToken);

                break;

            default: throw new ArgumentOutOfRangeException($"Invalid content typ {command.File.ContentType}");
        }
    }

    private async Task<IEnumerable<Booking>> SaveCamt(Guid partitionId,
                                                      Stream stream,
                                                      CancellationToken cancellationToken)
    {
        var newPayments = new List<Booking>();
        stream.Position = 0;
        var xml = XDocument.Load(stream);

        var camt = Camt053Parser.Parse(xml);

        var existingFile = await paymentFiles.FirstOrDefaultAsync(fil => fil.PartitionId == partitionId
                                                                      && fil.FileId == camt.FileId,
                                                                  cancellationToken);

        if (existingFile != null)
        {
            log.LogInformation("File with Id {camtFileId} already exists (PaymentFile.Id = {existingFileId})", camt.FileId, existingFile.Id);

            return newPayments;
        }

        if (configuration.Iban != camt.Account)
        {
            log.LogWarning("IBAN of camt {camt} does not match configured IBAN {config}.", camt.Account, configuration.Iban);
        }

        var paymentFile = new PaymentsFile
                          {
                              Id = Guid.CreateVersion7(),
                              PartitionId = partitionId,
                              Content = xml.ToString(),
                              FileId = camt.FileId,
                              AccountIban = camt.Account,
                              Balance = camt.Balance,
                              Currency = camt.Currency,
                              BookingsFrom = camt.BookingsFrom,
                              BookingsTo = camt.BookingsTo
                          };

        paymentFiles.Insert(paymentFile);

        foreach (var camtEntry in camt.Entries)
        {
            // dedup
            if (await bookings.AnyAsync(pmt => pmt.Reference == camtEntry.Reference
                                            && pmt.PartitionId == partitionId,
                                        cancellationToken))
            {
                continue;
            }

            var bookingId = Guid.NewGuid();

            var newPayment = camtEntry.Type == CreditDebit.DBIT
                ? new Booking
                  {
                      Id = bookingId,
                      PartitionId = partitionId,
                      Type = PaymentType.Outgoing,
                      PaymentsFileId = paymentFile.Id,
                      Info = camtEntry.Info,
                      Message = camtEntry.Message,
                      Amount = camtEntry.Amount,
                      BookingDate = camtEntry.BookingDate,
                      Currency = camtEntry.Currency,
                      Reference = camtEntry.Reference,
                      InstructionIdentification = camtEntry.InstructionIdentification,
                      RawXml = camtEntry.Xml,

                      Outgoing = new OutgoingPayment
                                 {
                                     Id = bookingId,
                                     CreditorName = camtEntry.CreditorName,
                                     CreditorIban = camtEntry.CreditorIban,
                                     Charges = camtEntry.Charges
                                 }
                  }
                : new Booking
                  {
                      Id = bookingId,
                      PartitionId = partitionId,
                      Type = PaymentType.Incoming,
                      PaymentsFileId = paymentFile.Id,
                      Info = camtEntry.Info,
                      Message = camtEntry.Message,
                      Amount = camtEntry.Amount,
                      BookingDate = camtEntry.BookingDate,
                      Currency = camtEntry.Currency,
                      Reference = camtEntry.Reference,
                      InstructionIdentification = camtEntry.InstructionIdentification,
                      RawXml = camtEntry.Xml,
                      Incoming = new IncomingPayment
                                 {
                                     Id = bookingId,
                                     DebitorName = camtEntry.DebitorName,
                                     DebitorIban = camtEntry.DebitorIban,
                                     Charges = camtEntry.Charges,
                                 }
                  };
            bookings.Insert(newPayment);
            newPayments.Add(newPayment);
        }

        changeTrigger.PublishEvent(new PaymentFileProcessed
                                   {
                                       PartitionId = partitionId,
                                       Account = camt.Account,
                                       Balance = camt.Balance,
                                       EntriesCount = camt.Entries.Count
                                   });

        //eventBus.Publish(new QueryChanged { PartitionId = partitionId, QueryName = nameof(BookingsByStateQuery) });

        return newPayments;
    }

    private async Task SaveCamtWithPaymentSlips(Guid eventId, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Booking slips are not implemented yet for ISO20022");
        //await using var zipStream = new GZipStream(stream, CompressionMode.Decompress);
        //await using var tarStream = new TarInputStream(zipStream, Encoding.UTF8);
        //var newLines = new List<Booking>();

        //while (await tarStream.GetNextEntryAsync(cancellationToken) is { } entry)
        //{
        //    var outStream = new MemoryStream();

        //    if (entry.Name.EndsWith(".xml"))
        //    {
        //        await tarStream.CopyEntryContentsAsync(outStream, cancellationToken);
        //        outStream.Position = 0;
        //        newLines.AddRange(await SaveCamt(eventId, outStream, cancellationToken));
        //    }
        //    else if (entry.Name.EndsWith(".tiff"))
        //    {
        //        var fileInfo = new FileInfo(entry.Name);
        //        var matches = Regex.Match(entry.Name, PostfinancePaymentSlipFilenameRegex);
        //        await tarStream.CopyEntryContentsAsync(outStream, cancellationToken);
        //        outStream.Position = 0;
        //        var reference = matches.Groups["ID"].Value;
        //        var iban = matches.Groups["IBAN"].Value;
        //        byte[] binary;
        //        string extension;

        //        //if (fileInfo.Extension is ".tiff" or ".tif")
        //        //{
        //        //    // chrome doesn't support tiff, so convert it to png
        //        //    extension = "image/png";
        //        //    var pngStream = new MemoryStream();
        //        //    new Bitmap(outStream).Save(pngStream, ImageFormat.Png);
        //        //    binary = pngStream.ToArray();
        //        //}
        //        //else
        //        {
        //            extension = ConvertExtensionToContentType(fileInfo.Extension);
        //            binary = outStream.ToArray();
        //        }

        //        var paymentSlip = new PaymentSlip
        //                          {
        //                              EventId = eventId,
        //                              ContentType = extension,
        //                              FileBinary = binary,
        //                              Filename = entry.Name,
        //                              Reference = reference
        //                          };
        //        await paymentSlips.InsertOrUpdateEntity(paymentSlip, cancellationToken);

        //        eventBus.Publish(new PaymentSlipReceived
        //                         {
        //                             EventId = eventId,
        //                             Reference = reference,
        //                             PaymentSlipId = paymentSlip.Id
        //                         });
        //    }
        //}
    }

    private async Task TrySavePaymentSlipImage(Guid eventId,
                                               Stream fileStream,
                                               string filename,
                                               string contentType)
    {
        throw new NotImplementedException("Booking slips are not implemented yet for ISO20022");

        //var reference = filename.Split('.').First();

        //var paymentSlip = new PaymentSlip
        //                  {
        //                      Id = Guid.NewGuid(),
        //                      EventId = eventId,
        //                      FileBinary = fileStream.ToArray(),
        //                      Filename = filename,
        //                      Reference = reference,
        //                      ContentType = contentType
        //                  };
        //await paymentSlips.InsertOrUpdateEntity(paymentSlip);

        //eventBus.Publish(new PaymentSlipReceived
        //                 {
        //                     EventId = eventId,
        //                     Reference = reference,
        //                     PaymentSlipId = paymentSlip.Id
        //                 });
    }
}