import { OrderLine } from './orderLine.model';

export interface RequestModel {
  id?: number;
  requestingCenterId: number;
  urgencyLevel: string;
  requestDate: Date | string;
  requestingCenter?: {
    id: number;
    name: string;
  };
  orderLines: OrderLine[];
}
