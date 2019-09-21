import Vue from 'vue';
import App from './App.vue';
import router from './router';
import axios, { AxiosRequestConfig } from 'axios';
import VueAxios from 'vue-axios';

Vue.config.productionTip = false ;

// need to exernalize configuration for this
axios.defaults.baseURL = 'http://localhost:5100/api';
axios.interceptors.request.use((config: AxiosRequestConfig) => {

  // detect sever side rendering
  if (typeof window === undefined) {
    return config;
  }

  const authToken = window.localStorage.getItem('authToken');
  if (authToken) {
    config.headers.Authorization = `Bearer ${authToken}`;
  }
  return config;
});

Vue.use(VueAxios, axios);

const app = new Vue({
  router,
  render: (h) => h(App),
}).$mount('#app');

export { app, router };
