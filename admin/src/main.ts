import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import router from './router'
import { setupMockAdapter } from './mock/handler'
import App from './App.vue'
import './assets/css/tailwind.css'

// Initialize mock data adapter in development
if (import.meta.env.VITE_USE_MOCK === 'true') {
  setupMockAdapter()
}

const app = createApp(App)

// Register all Element Plus icons
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(createPinia())
app.use(router)
app.use(ElementPlus)
app.mount('#app')
