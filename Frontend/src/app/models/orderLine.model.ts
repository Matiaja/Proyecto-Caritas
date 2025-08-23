import { DonationRequest } from "./donationRequest.model";
import { Product } from "./product.model";
import { RequestModel } from "./request.model";

export interface OrderLine {
  id?: number;
  requestId?: number;
  donationRequests?: DonationRequest[];
  productId: number;
  quantity: number;
  description: string;
  status?: string;
  productName?: string;
  product?: Product;
  request?: RequestModel;
}
