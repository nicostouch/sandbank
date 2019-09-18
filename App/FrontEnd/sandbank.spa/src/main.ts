import Vue from 'vue';
import App from './App.vue';
import router from './router';
import axios from 'axios';
import VueAxios from 'vue-axios';

Vue.config.productionTip = false ;
axios.defaults.baseURL = 'http://localhost:5100/api';
Vue.use(VueAxios, axios);

const app = new Vue({
  router,
  render: (h) => h(App),
}).$mount('#app');

export { app, router };