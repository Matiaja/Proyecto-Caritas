export interface DonationRequest {
  id?: number;
  orderLineId: number;
  assignedCenterId: number;
  quantity: number;
  status?: string;
}
