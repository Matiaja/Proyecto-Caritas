import { DonationRequest } from "./donationRequest.model";

export interface OrderLine {
  id?: number;
  requestId?: number;
  donationRequests?: DonationRequest[];
  productId: number;
  quantity: number;
  description: string;
  productName?: string;
}
