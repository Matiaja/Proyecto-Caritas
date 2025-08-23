import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { SpinnerService } from '../../services/spinner/spinner.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const spinnerService = inject(SpinnerService);

  if (req.url.includes('chat/')) {
    return next(req); // no mostrar spinner
  }

  spinnerService.show();

  return next(req).pipe(
    finalize(() => spinnerService.hide())
  );
};
