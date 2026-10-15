<template>
  <div class="menu-bar-container">
    <!-- logo -->
    <div class="logo"
         :style="{'background':themeColor, 'display': fullscreen? 'none':''}"
         :class="collapse?'menu-bar-collapse-width':'menu-bar-width'"
         @click="$router.push('/')">
      <img v-if="collapse" src="@/assets/codelab.jpg"/>
      <div>{{ collapse ? '' : appName }}</div>
    </div>
    <!-- Menu điều hướng -->
    <el-menu ref="navMenu"
             :style="{'display': fullscreen? 'none':''}"
             :default-active="$route.path"
             :class="collapse?'menu-bar-collapse-width':'menu-bar-width'"
             :collapse="collapse"
             :collapse-transition="false"
             :unique-opened="false"
             router
    >
      <el-menu-item index="/">
        <i class="el-icon-s-home"></i>
        <span slot="title">Trang chủ</span>
      </el-menu-item>
      <el-submenu index="blog">
        <template slot="title">
          <i class="el-icon-s-grid"></i>
          <span>Blog</span>
        </template>
        <el-menu-item-group title="Quản lý danh mục">
          <el-menu-item index="/category/list">Danh sách danh mục</el-menu-item>
          <el-menu-item index="/category/tree">Cây danh mục</el-menu-item>
          <el-menu-item index="/category/featured">Danh mục đề xuất</el-menu-item>
        </el-menu-item-group>
        <el-menu-item-group title="Quản lý bài viết">
          <el-menu-item index="/post/list">Danh sách bài viết</el-menu-item>
          <el-menu-item index="/post/featured">Bài viết đề xuất</el-menu-item>
          <el-menu-item index="/post/top">Bài viết được ghim</el-menu-item>
        </el-menu-item-group>
        <el-menu-item-group title="Quản lý bình luận">
          <el-menu-item index="/comment/list">Quản lý bình luận</el-menu-item>
        </el-menu-item-group>
        <el-menu-item-group title="Công nghệ đỉnh">
          <el-menu-item index="/post/upload">Đóng gói và tải lên</el-menu-item>
        </el-menu-item-group>
      </el-submenu>
      <el-submenu index="photography">
        <template slot="title">
          <i class="el-icon-camera"></i>
          <span>Nhiếp ảnh</span>
        </template>
        <el-menu-item-group title="Quản lý ảnh">
          <el-menu-item index="/photo/list">Danh sách ảnh</el-menu-item>
          <el-menu-item index="/photo/featured">Ảnh đề xuất</el-menu-item>
        </el-menu-item-group>
      </el-submenu>
      <el-menu-item index="/visit_record/list">
        <i class="el-icon-menu"></i>
        <span slot="title">Nhật ký truy cập</span>
      </el-menu-item>
      <el-menu-item index="/config/list">
        <i class="el-icon-s-tools"></i>
        <span slot="title">Trung tâm cấu hình</span>
      </el-menu-item>
      <el-submenu index="link">
        <template slot="title">
          <i class="el-icon-link"></i>
          <span>Liên kết bạn bè</span>
        </template>
        <el-menu-item-group title="Quản lý liên kết bạn bè">
          <el-menu-item index="/link/list">Liên kết bạn bè</el-menu-item>
          <el-menu-item index="/link_exchange/list">Quản lý yêu cầu trao đổi liên kết</el-menu-item>
        </el-menu-item-group>
      </el-submenu>
    </el-menu>
  </div>
</template>

<script>
import {mapState} from 'vuex'

export default {
  computed: {
    ...mapState({
      appName: state => state.app.appName,
      themeColor: state => state.app.themeColor,
      collapse: state => state.app.collapse,
      fullscreen: state => state.app.fullscreen,
    }),
    mainTabs: {
      get() {
        return this.$store.state.tab.mainTabs
      },
      set(val) {
        this.$store.commit('updateMainTabs', val)
      }
    },
    mainTabsActiveName: {
      get() {
        return this.$store.state.tab.mainTabsActiveName
      },
      set(val) {
        this.$store.commit('updateMainTabsActiveName', val)
      }
    }
  },
  watch: {
    $route: 'handleRoute'
  },
  created() {
    this.handleRoute(this.$route)
  },
  methods: {
    // Xử lý thao tác route
    handleRoute(route) {
      // tab Tab được chọn, nếu chưa tồn tại thì thêm trước
      let tab = this.mainTabs.filter(item => item.name === route.name)[0]
      if (!tab) {
        tab = {
          name: route.name,
          title: route.name,
          icon: route.meta.icon,
          fullPath: route.fullPath,
          path: route.path,
          params: route.params,
          query: route.query
        }
        this.mainTabs = this.mainTabs.concat(tab)
      }
      this.mainTabsActiveName = tab.name
      // Đồng bộ làm nổi bật menu khi chuyển tab
      if (this.$refs.navMenu != null) {
        this.$refs.navMenu.activeIndex = '' + route.meta.index
        this.$refs.navMenu.initOpenedMenu()
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.menu-bar-container {
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  z-index: 1020;

  .el-menu {
    position: absolute;
    top: 60px;
    bottom: 0;
    text-align: left;
  }

  .logo {
    position: absolute;
    top: 0;
    height: 60px;
    line-height: 60px;
    background: #545c64;
    cursor: pointer;

    img {
      width: 40px;
      height: 40px;
      border-radius: 0;
      margin: 10px 10px 10px 10px;
      float: left;
    }

    div {
      font-size: 22px;
      color: white;
      text-align: left;
      padding-left: 20px;
    }
  }

  .menu-bar-width {
    width: 200px;
  }

  .menu-bar-collapse-width {
    width: 65px;
  }
}
</style>
