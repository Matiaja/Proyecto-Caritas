import { AbstractControl, ValidationErrors } from '@angular/forms';

export function expirationDateValidator(creationDateControlName: string) {
  return (control: AbstractControl): ValidationErrors | null => {
    const expirationDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const creationDateControl = control?.parent?.get(creationDateControlName);
    const creationDate = creationDateControl ? new Date(creationDateControl.value) : null;

    if (control.value == '' || !control.value) {
      return null;
    }

    if (expirationDate < today) {
      return { expirationDateInvalid: 'La fecha de expiración no puede ser menor a hoy.' };
    }

    if (creationDate && expirationDate < creationDate) {
      return {
        expirationDateInvalid: 'La fecha de expiración no puede ser menor a la fecha de creación.',
      };
    }

    return null;
  };
}
