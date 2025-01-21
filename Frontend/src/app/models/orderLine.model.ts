export interface OrderLine {
  id: number;
  requestId: number;
  donationRequestId: number;
  productId: number;
  quantity: number;
  description: string;
}