export abstract class AppEngineNavigator {
  public abstract getSourceUrl(sourceType: string, sourceId: string);
  public abstract goToSettlePaymentUrl(registrationId: string): void;
  public abstract getSettlePaymentUrl(paymentId: string): string;
}
