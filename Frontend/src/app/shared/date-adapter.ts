import { Inject, Injectable, Optional } from "@angular/core";
import { MAT_DATE_LOCALE } from "@angular/material/core";
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import moment from 'moment';
import 'moment/locale/es'; 

@Injectable()
export class CustomDateAdapter extends MomentDateAdapter {
  constructor(@Optional() @Inject(MAT_DATE_LOCALE) dateLocale: string) {
    super(dateLocale);
  }

  override parse(value: any): moment.Moment | null {
    if (value && typeof value === 'string') {
      // Intenta parsear diferentes formatos
      const formats = ['DD/MM/YYYY', 'DD-MM-YYYY', 'DD.MM.YYYY', 'D/M/YYYY', 'D-M-YYYY'];
      const date = moment(value, formats, true);
      return date.isValid() ? date : null;
    }
    return value ? moment(value, moment.ISO_8601, true) : null;
  }

  // Hace que el calendario empiece en domingo
  override getFirstDayOfWeek(): number {
    return 0; // 0 = domingo
  }

  // Asegura que moment use el locale español
  override setLocale(locale: string): void {
    super.setLocale(locale);
    moment.locale('es');
  }
}

export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};