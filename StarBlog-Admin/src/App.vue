<template>
  <div id="app" :class="{ 'has-navbar': hasNavbar }">
<!--    <img src="./assets/logo.png">-->
    <router-view/>
  </div>
</template>

<script>
export default {
  name: 'App',
  computed: {
    hasNavbar() {
      // Các trang như đăng nhập, 404 không cần không gian thanh điều hướng
      const fullscreenRoutes = ['/login', '/Login', '/404']
      return !fullscreenRoutes.includes(this.$route.path)
    }
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}

/* Chỉ thêm khoảng cách phía trên cho các trang cần thanh điều hướng */
#app.has-navbar {
  margin-top: 60px;
}
</style>
