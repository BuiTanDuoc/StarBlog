<template>
  <div class="container">
    <!-- Thanh menu điều hướng -->
    <navbar></navbar>
    <!-- Vùng đầu trang -->
    <header-bar></header-bar>
    <!-- Vùng nội dung chính -->
    <main-content></main-content>
  </div>
</template>

<script>
import HeaderBar from "./HeaderBar"
import Navbar from "./Navbar"
import MainContent from "./MainContent"

export default {
  // name: 'Home',
  components: {
    HeaderBar,
    Navbar,
    MainContent
  }
};
</script>

<style scoped lang="scss">
.container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}
</style>
