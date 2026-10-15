import Vue from 'vue'
import Router from 'vue-router'
import Login from "@/views/Login";
import Framework from "@/views/Framework";
import NotFound from '@/views/404'
import Home from "@/views/Home"
import Categories from "@/views/Category/Categories"
import CategoriesTree from "@/views/Category/CategoriesTree"
import CategoryDetail from "@/views/Category/CategoryDetail"
import FeaturedCategories from "@/views/Category/FeaturedCategories"
import Comments from '@/views/Comment/Comments.vue'
import Posts from "@/views/Blog/Posts"
import TopPost from "@/views/Blog/TopPost"
import FeaturedPosts from "@/views/Blog/FeaturedPosts"
import EditPost from "@/views/Blog/EditPost";
import UploadPost from "@/views/Blog/UploadPost";
import Photos from "@/views/Photography/Photos"
import FeaturedPhotos from "@/views/Photography/FeaturedPhotos"
import VisitRecordList from "@/views/VisitRecord/List"
import ConfigList from "@/views/Config/List"
import LinkList from '@/views/Link/List'
import LinkExchangeList from '@/views/LinkExchange/List.vue'
import * as auth from '@/utils/auth'


const originalPush = Router.prototype.push

Router.prototype.push = function push(location) {
  return originalPush.call(this, location).catch(err => err)
}

Vue.use(Router)

const router = new Router({
  mode: 'hash',
  routes: [
    {
      path: '/',
      component: Framework,
      children: [
        {
          path: '',
          name: 'Trang chủ hệ thống',
          component: Home,
          meta: {
            icon: 'fa fa-home fa-lg',
            index: 0
          }
        },
        {path: 'category/list', name: 'Danh sách danh mục', component: Categories},
        {path: 'category/tree', name: 'Cây danh mục', component: CategoriesTree},
        {path: 'category/detail/:id', name: 'Chi tiết danh mục', component: CategoryDetail},
        {path: 'category/featured', name: 'Danh mục đề xuất', component: FeaturedCategories},
        {path: 'comment/list', name: 'Duyệt bình luận', component: Comments},
        {path: 'post/list', name: 'Danh sách bài viết', component: Posts},
        {path: 'post/featured', name: 'Bài viết đề xuất', component: FeaturedPosts},
        {path: 'post/top', name: 'Bài viết được ghim', component: TopPost},
        {path: 'post/edit/:id', name: 'Sửa bài viết', component: EditPost},
        {path: 'post/new', name: 'Thêm bài viết mới', component: EditPost},
        {path: 'post/upload', name: 'Tải bài viết lên', component: UploadPost},
        {path: 'photo/list', name: 'Danh sách ảnh', component: Photos},
        {path: 'photo/featured', name: 'Ảnh đề xuất', component: FeaturedPhotos},
        {path: 'visit_record/list', name: 'Lịch sử truy cập', component: VisitRecordList},
        {path: 'config/list', name: 'Trung tâm cấu hình', component: ConfigList},
        {path: 'link/list', name: 'Quản lý liên kết bạn bè', component: LinkList},
        {path: 'link_exchange/list', name: 'Yêu cầu trao đổi liên kết', component: LinkExchangeList},
      ]
    },
    {path: '/Login', name: 'Login', component: Login},
    {path: '/404', name: 'NotFound', component: NotFound},
  ]
})

// Route guard
router.beforeEach((to, from, next) => {
  if (auth.isExpired()) {
    auth.logout()
    router.push('/login')
  }

  if (to.path === '/login') {
    // Truy cập trang đăng nhập, nếu đã đăng nhập thì chuyển đến trang chủ
    if (auth.isLogin()) next({path: '/'})
    else next()
  } else {
    if (!auth.isLogin()) {
      // Nếu truy cập trang không phải đăng nhập mà không có thông tin phiên người dùng, nghĩa là chưa đăng nhập, thì chuyển đến trang đăng nhập
      next({path: '/login'})
    }
    next()
  }
})

export default router
