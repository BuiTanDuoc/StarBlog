// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'

import App from './App'
import router from './router'
import api from './http'
import Exceptionless from './utils/exceptionless'
import ErrorHandler from "./utils/errorHandler"
import global from './utils/global'
import store from './store'

// Import một phần css của bootstrap
import 'bootstrap/dist/css/bootstrap-reboot.css'
import 'bootstrap/dist/css/bootstrap-utilities.css'

// ElementUI
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'
// FontAwesome
import 'font-awesome/css/font-awesome.min.css'

// DataV Biểu đồ
import dataV from '@jiaminghi/data-view'

// markdown Trình soạn thảo
import VMdEditor from '@kangc/v-md-editor/lib/codemirror-editor';
import '@kangc/v-md-editor/lib/style/codemirror-editor.css';
import githubTheme from '@kangc/v-md-editor/lib/theme/github.js';
import '@kangc/v-md-editor/lib/theme/style/github.css';
// highlightjs
import hljs from 'highlight.js';
// codemirror Tài nguyên liên quan đến trình soạn thảo
import Codemirror from 'codemirror';
// mode
import 'codemirror/mode/markdown/markdown';
import 'codemirror/mode/javascript/javascript';
import 'codemirror/mode/css/css';
import 'codemirror/mode/htmlmixed/htmlmixed';
import 'codemirror/mode/vue/vue';
// edit
import 'codemirror/addon/edit/closebrackets';
import 'codemirror/addon/edit/closetag';
import 'codemirror/addon/edit/matchbrackets';
// placeholder
import 'codemirror/addon/display/placeholder';
// active-line
import 'codemirror/addon/selection/active-line';
// scrollbar
import 'codemirror/addon/scroll/simplescrollbars';
import 'codemirror/addon/scroll/simplescrollbars.css';
// style
import 'codemirror/lib/codemirror.css';

VMdEditor.Codemirror = Codemirror;
VMdEditor.use(githubTheme, {
  Hljs: hljs,
});
Vue.use(VMdEditor);

Vue.config.productionTip = false

Vue.use(dataV)
// Import Element
Vue.use(ElementUI)
// Import module API
Vue.use(api)
// Exceptionless Module
Vue.use(Exceptionless)
// Module xử lý bất đồng bộ toàn cục
Vue.use(ErrorHandler)

// Gắn module cấu hình toàn cục
Vue.prototype.global = global

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  store,
  components: {App},
  template: '<App/>'
})
