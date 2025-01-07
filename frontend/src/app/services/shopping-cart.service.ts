import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Product } from '../models/product';
import { environment } from '../../environments/environment';
import { CartContent } from '../models/cart-content';

@Injectable({
  providedIn: 'root'
})
export class ShoppingCartService {

  total: number = 0
  contProduct: number = 0
  shoppingCartProducts: Product[] = []
  productsToBuy: CartContent[] = [];
  isSaved: boolean = true

  constructor(private apiService: ApiService) { }


  async addToCart(product: Product, quantity = 1) {

    const count = quantity;


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
      this.contProduct = allProducts.length

    }
    else {
      localStorage.removeItem("shoppingCart")
      const cartContent = new CartContent(product.id, count, product)
      // Envía el objeto `cartContent` directamente, sin envolverlo en un objeto con clave `cartContent`
      const result = await this.apiService.post("ShoppingCart/addProductOrChangeQuantity", cartContent, { "add": true })

      if (result.data) {
        this.contProduct = parseInt(result.data)
      }
    }

    //this.shoppingCartService.getShoppingCartCount()

  }

  getLocalStorageCart() {
    //this.shoppingCartProducts = [];
    const productsRaw = localStorage.getItem("shoppingCart");
    if (productsRaw) {
      this.shoppingCartProducts = JSON.parse(productsRaw);
    }

  }

  async syncronizeCart(add: boolean) {
    const productsRaw = localStorage.getItem("shoppingCart");
    let products: Product[] = []
    if (productsRaw == null) {
      return
    }
    products = JSON.parse(productsRaw);

    if (this.apiService.jwt !== "" && products.length > 0) {
      console.log("Sincronizando productos locales al carrito del backend...");

      await this.uploadCart(products, add)

      localStorage.removeItem("shoppingCart");
    }
  }


  async getShoppingCart() {

    this.shoppingCartProducts = [];



    // Podríamos optimizar esto haciendo que en el login ponga el contenido del carrito en el localStorage, de manera que no tenga que hacer peticiones
    // Sin embargo, si un admin borra o cambia un producto, cagamos
    if (this.apiService.jwt !== "") {
      const result = await this.apiService.get("ShoppingCart", {}, 'json');
      if (result.data) {
        const data: any = result.data;
        const cartContent: any[] = data.cartContent;
        for (const product of cartContent) {
          let p = product.product
          p.total = product.quantity
          p = this.addCorrectPath(p)
          this.shoppingCartProducts.push(p);
        }
      }
      console.log("CARRITO SINCRONIZADO: ", this.shoppingCartProducts);


    }
    else {
      this.getLocalStorageCart();
    }


    this.contProduct = this.shoppingCartProducts.length

  }

  async saveShoppingCart(): Promise<void> {

    if (!this.isSaved) {
      //Guarda los cambios del carrito
      if (this.apiService.jwt == "") {
        localStorage.setItem("shoppingCart", JSON.stringify(this.shoppingCartProducts));
      }
      else {

        var cart: CartContent[] = []

        this.shoppingCartProducts.forEach(product => {
          cart.push(new CartContent(product.id, product.total, product))
        });

        await this.apiService.post("ShoppingCart/save", cart)

      }
    }

    console.log("carrito guardado")

    this.isSaved = true
  }

  async uploadCart(products: Product[], add: boolean) {
    var cart: CartContent[] = []

    products.forEach(product => {
      cart.push(new CartContent(product.id, product.total, product))
    });

    await this.apiService.post("ShoppingCart/save", cart, {"add" : add})
  }

  async deleteProduct(productId: number) {
    const index = this.shoppingCartProducts.findIndex(product => product.id === productId);

    if (index !== -1) {
      const product = this.shoppingCartProducts[index];

      if (product.total > 1) {
        product.total -= 1;
      } else {
        this.shoppingCartProducts.splice(index, 1);
      }

      if (this.apiService.jwt == "") {
        this.deleteFromArray(product, false)
      }
      else {
        await this.apiService.delete("ShoppingCart", { productId })
        this.getShoppingCart()
      }
    }

  }


  findProductInArray(id: number): Product {
    const index = this.shoppingCartProducts.findIndex(product => product.id === id);
    const product = this.shoppingCartProducts[index];
    return product;
  }


  deleteFromArray(product: Product, showAlert: boolean) {
    const index = this.shoppingCartProducts.findIndex(p => p.id === product.id);
    this.shoppingCartProducts.splice(index, 1);
    localStorage.setItem("shoppingCart", JSON.stringify(this.shoppingCartProducts))
    if (showAlert) {
      alert("Uno o varios productos han sido eliminados por falta de stock")
    }
  }




  async getShoppingCartCount() {
    // Si quisiésemos podríamos recorrer el Array e ir sumando las cantidades con un for, ya como veáis
    if (this.apiService.jwt != "" && this.apiService.jwt != null) {
      const result = await this.apiService.get("ShoppingCart", {}, 'json');
      if (result.data) {
        const data: any = result.data;
        console.log("DATA MONDONGO: ", data)
        this.total = data.cartContent.length
      }
    }
    else {
      const cart = localStorage.getItem("shoppingCart")
      if (cart) {
        const cartObject = JSON.parse(cart)
        console.log("CART OBJECT: ", cartObject)
        this.total = cartObject.length
      }
    }
  }

  addCorrectPath(product: Product) {
    for ( var image of product.images) {
      image = environment.imageRouteBasic + image 
    }
    return product
  }
}
