import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { format, formatDistanceStrict, isValid, parseISO, isSameDay, isSameMonth, isSameYear } from 'date-fns';
import { de, enUS } from 'date-fns/locale';

@Pipe({ name: 'datePeriod' })
export class DatePeriodPipe implements PipeTransform 
{
  constructor(private translateService: TranslateService) { }

  public transform(fromDate: Date | string | null, untilDate: Date | string | null, format?: 'short' | 'long' | 'relative'): string 
  {
    if (!fromDate || !untilDate) 
    {
      return '';
    }

    const from = this.parseDate(fromDate);
    const until = this.parseDate(untilDate);

    if (!isValid(from) || !isValid(until)) 
    {
      return '';
    }

    const formatType = format || 'long';
    const locale = this.getLocale();

    return this.formatPeriod(from, until, formatType, locale);
  }

  private parseDate(date: Date | string): Date 
  {
    if (typeof date === 'string') 
    {
      return parseISO(date);
    }
    return date;
  }

  private getLocale() 
  {
    const currentLang = this.translateService.currentLang || 'en';
    return currentLang === 'de' ? de : enUS;
  }

  private formatPeriod(from: Date, until: Date, formatType: string, locale: any): string 
  {
    // Same day
    if (isSameDay(from, until)) 
    {
      const dateStr = format(from, 'PPP', { locale });
      return this.translateService.instant('DATE_PERIOD.SAME_DAY', { date: dateStr });
    }

    switch (formatType) 
    {
      case 'short':
        return this.formatShort(from, until, locale);
      case 'relative':
        return this.formatRelative(from, until);
      case 'long':
      default:
        return this.formatLong(from, until, locale);
    }
  }

  private formatShort(from: Date, until: Date, locale: any): string 
  {
    if (isSameMonth(from, until)) 
    {
      const fromDay = format(from, 'd', { locale });
      const untilDay = format(until, 'd', { locale });
      const monthYear = format(from, 'MMM yyyy', { locale });
      return `${fromDay} - ${untilDay} ${monthYear}`;
    }

    if (isSameYear(from, until)) 
    {
      const fromStr = format(from, 'd MMM', { locale });
      const untilStr = format(until, 'd MMM yyyy', { locale });
      return `${fromStr} - ${untilStr}`;
    }

    const fromStr = format(from, 'MMM yyyy', { locale });
    const untilStr = format(until, 'MMM yyyy', { locale });
    return `${fromStr} - ${untilStr}`;
  }

  private formatLong(from: Date, until: Date, locale: any): string 
  {
    if (isSameMonth(from, until)) 
    {
      const fromDay = format(from, 'd', { locale });
      const untilDay = format(until, 'd', { locale });
      const monthYear = format(from, 'MMMM yyyy', { locale });
      return this.translateService.instant('DATE_PERIOD.SAME_MONTH', { 
        fromDay, 
        untilDay, 
        monthYear 
      });
    }

    if (isSameYear(from, until)) 
    {
      const fromStr = format(from, 'd MMMM', { locale });
      const untilStr = format(until, 'd MMMM yyyy', { locale });
      return this.translateService.instant('DATE_PERIOD.SAME_YEAR', { 
        from: fromStr, 
        until: untilStr 
      });
    }

    const fromStr = format(from, 'PPP', { locale });
    const untilStr = format(until, 'PPP', { locale });
    return this.translateService.instant('DATE_PERIOD.DIFFERENT_YEARS', { 
      from: fromStr, 
      until: untilStr 
    });
  }

  private formatRelative(from: Date, until: Date): string 
  {
    const distance = formatDistanceStrict(from, until, { 
      locale: this.getLocale() 
    });
    
    return this.translateService.instant('DATE_PERIOD.DURATION', { duration: distance });
  }
}