export type NotificationType = 
  'Assignment' | 'Acceptance' | 'Rejection' | 
  'Shipping' | 'Receipt' | 'System';

export const NOTIFICATION_ACTIONS = {
  Assignment: [
    {
      label: 'Aceptar',
      action: 'accept',
      icon: 'check_circle'
    },
    {
      label: 'Rechazar',
      action: 'reject',
      icon: 'cancel'
    }
  ],
  Acceptance: [
    {
      label: 'Marcar como Enviado',
      action: 'mark_as_shipped',
      icon: 'local_shipping'
    }
  ],
  Shipping: [
    {
      label: 'Confirmar Recepción',
      action: 'confirm_receipt',
      icon: 'done_all'
    }
  ],
  Receipt: [], // No necesita acciones
  Rejection: [
    {
      label: 'Reasignar',
      action: 'reassign',
      icon: 'replay'
    }
  ],
  System: [] // Notificaciones informativas
};