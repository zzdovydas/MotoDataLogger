import './assets/main.css';

import PrimeVue from 'primevue/config';
import Button from "primevue/button";
import InputText from 'primevue/inputtext';
import IconField from 'primevue/iconfield';
import InputIcon from 'primevue/inputicon';
import Password from 'primevue/password';
import Checkbox from 'primevue/checkbox';
import Chart from 'primevue/chart';
import Aura from '@primeuix/themes/aura';
import 'primeicons/primeicons.css';
import axios from 'axios';

import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap";

import { createApp } from 'vue';
import App from './App.vue';
import router from './router';

const app = createApp(App);

app.use(router);

app.use(PrimeVue, {
    theme: {
        preset: Aura
    }
});
app.component('Button', Button);
app.component('InputText', InputText);
app.component('IconField', IconField);
app.component('InputIcon', InputIcon);
app.component('Password', Password);
app.component('Checkbox', Checkbox);

// Check if a token exists in localStorage and set it in the axios headers
const token = localStorage.getItem('jwtToken');
if (token) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
}

app.mount('#app');
