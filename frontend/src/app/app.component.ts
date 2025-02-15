import { Component, HostListener, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ShoppingCartService } from './services/shopping-cart.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

export class AppComponent implements OnInit {
  title = 'frontend';

  constructor(private shoppingCartService: ShoppingCartService) { }

  async ngOnInit(): Promise<void> {
    console.log(window.ethereum);

  }


  //Guarda el carrito cuando cierra la página
  @HostListener('window:beforeunload', ['$event'])
  async handleBeforeUnload(event: BeforeUnloadEvent): Promise<void> {
    if (this.shoppingCartService.isSaved) {
      const confirmationMessage = "Guardando el carrito...";
      event.returnValue = confirmationMessage;

      // Establece el mensaje de confirmación 
      await this.shoppingCartService.saveShoppingCart();
    }


  }
}

declare global {
  interface Window {
    ethereum: any;
  }
}

