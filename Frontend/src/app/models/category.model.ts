import { Product } from "./product.model";

export interface Category {
  id: number;
  name: string;
  description: string;
  products?: Product[]; // Optional property to hold products associated with the category
}
