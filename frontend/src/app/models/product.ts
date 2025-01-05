import { Category } from "./category";
import { Review } from "./review";

export interface Product {
    id: number,
    name: string,
    description: string,
    price: number,
    stock: number,
    average: number,
    images: string[],
    categoryId: number,
    category: Category,
    reviews: Review[],
    total: number // Para el carrito
}
