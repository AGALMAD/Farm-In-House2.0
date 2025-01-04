import { Component, HostListener } from '@angular/core';
import { HeaderComponent } from '../../components/header/header.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeaderComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  //Lo pongo tipo map para que tenga mejor legibilidad
  PRODUCTS = {
    FRUIT: {
      title: 'frutas',
      imageUrl: '/assets/main/cesta-frutas.jpg',
    },
    VEGETABLE: {
      title: 'verduras',
      imageUrl: '/assets/main/cesta-verduras.jpg',
    },
    MEAT: {
      title: 'carnes',
      imageUrl: '/assets/main/carne-ecologica.jpg',
    },
  };

  //Pasa los valores a un array para poder iterarlo en el html
  productArray = Object.entries(this.PRODUCTS).map(([key, value]) => ({
    key,
    ...value,
  }));

  constructor(private router: Router) {}

  goToRoute(route: string) {
    this.router.navigateByUrl(route);
  }
}
