import { Component, LOCALE_ID, OnInit } from '@angular/core';
import { HeaderComponent } from '../../components/header/header.component';
import { ActivatedRoute } from '@angular/router';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ApiService } from '../../services/api.service';
import { CartContent } from '../../models/cart-content';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { NewReview } from '../../models/new-review';
import { ReviewService } from '../../services/review.service';
import { CommonModule } from '@angular/common';
import { ShoppingCart } from '../../models/shopping-cart';
import Swal from 'sweetalert2';
import { ShoppingCartService } from '../../services/shopping-cart.service';
import { QuantityModifierComponent } from '../../components/quantity-modifier/quantity-modifier.component';

// Pipe Import
import { CorrectDatePipe } from '../../pipes/correct-date.pipe';


@Component({
  selector: 'app-product-view',
  standalone: true,
  imports: [HeaderComponent, FormsModule, CommonModule, CorrectDatePipe, QuantityModifierComponent],
  //providers: [{provide: LOCALE_ID, useValue: 'es'}],
  templateUrl: './product-view.component.html',
  styleUrl: './product-view.component.css'
})
export class ProductViewComponent implements OnInit {

  protected count = 1;
  product: Product | null = null;
  routeParamMap$: Subscription | null = null;
  //prductReviews: Review[] = []
  div_text: String = "Prueba";

  constructor(private productService: ProductService, private activatedRoute: ActivatedRoute, private apiService: ApiService,
    private reviewService: ReviewService, private shoppingCartService: ShoppingCartService) { }

  ngOnInit(): void {
    this.getProduct()
  }

  async getProduct() {

    this.routeParamMap$ = this.activatedRoute.paramMap.subscribe(async paramMap => {
      const id = paramMap.get('id') as unknown as number;
      const result = await this.productService.getById(id)
      if (result != null) {
        this.product = result
        console.log("PRODUCTO: ", this.product)
      }
    });

  }

  isLogged() {
    let boolean = false;
    if (this.apiService.jwt != null && this.apiService.jwt != "") {
      boolean = true
    }
    return boolean
  }

  async addReview() {
    const reviewTextElement = document.getElementById("review-text") as HTMLTextAreaElement | null; //Elemento del textArea

    if (reviewTextElement == null || reviewTextElement?.value.trim() === "" || this.product == null) {
      var alert_div = document.getElementById("alert-div");
      this.div_text = "No has hecho ningun comentario";
      alert_div?.classList.remove("alert-div-none");
      alert_div?.classList.add("alert-div");
    } else {
      const newReview = new NewReview(reviewTextElement.value, this.product.id, new Date().toISOString());

      console.log(newReview)

      await this.reviewService.addReview(newReview);

      if (reviewTextElement) {
        reviewTextElement.value = "";
      }

      this.getProduct();
    }

  }

  //Actualiza el contador del producto cuando se actualiza en el componente
  async onCountChange(event: { productId: number, newCount: number }) {
    const { productId, newCount } = event;
    this.count = newCount;
  }


  async addToCart(product: Product) {

    const count = this.count;


    if (this.apiService.jwt == "") {
      let allProducts: Product[] = []
      const productsLocalStore = localStorage.getItem("shoppingCart")
      if (productsLocalStore) {
        allProducts = JSON.parse(productsLocalStore)
        const index = allProducts.findIndex(p => p.id === product.id);
        let newProduct = product
        if (index != -1) {
          // Si el producto ya existe en el carrito, se actualiza la cantidad
          newProduct = allProducts[index];
          newProduct.total += count;
        }
        else {
          //Si es un nuevo producto lo añade con la cantidad asignada
          newProduct.total = count;
          allProducts.push(product);
        }
      }
      else {
        product.total = count
        allProducts.push(product)
      }
      localStorage.setItem("shoppingCart", JSON.stringify(allProducts))

      //Contador en local storage del número de productos en el carrito
      this.shoppingCartService.contProduct = allProducts.length

    }
    else {
      localStorage.removeItem("shoppingCart")
      const cartContent = new CartContent(product.id, count, product)
      // Envía el objeto `cartContent` directamente, sin envolverlo en un objeto con clave `cartContent`
      const result = await this.apiService.post("ShoppingCart/addProductOrChangeQuantity", cartContent, { "add": true })

      if (result.data) {
        this.shoppingCartService.contProduct = parseInt(result.data)
      }
    }

    //this.shoppingCartService.getShoppingCartCount()

  }

  mostraralert() {
    Swal.fire({
      title: 'Producto añadido',
      text: 'Se ha añadido el producto correctamente',
      icon: 'success',
      confirmButtonText: 'Salir'
    })
  }


  ngOnDestroy(): void {
    this.routeParamMap$?.unsubscribe();

  }
}
