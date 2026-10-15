<template>
  <div id="main-container" class="main-container"
       :class="$store.state.app.collapse?'position-collapse-left':'position-left'">
    <!-- Tab -->
    <!-- Tab -->
    <div class="tab-container" :style="{'display': fullscreen? 'none':''}">
      <el-tabs
        class="tabs text-center"
        :class="$store.state.app.collapse?'position-collapse-left':'position-left'"
        v-model="mainTabsActiveName"
        :closable="true" type="card"
        @tab-click="selectedTabHandle"
        @tab-remove="removeTabHandle"
      >
        <el-dropdown class="tabs-tools" :show-timeout="0" trigger="hover">
          <div style="font-size:20px;width:50px;"><i class="el-icon-arrow-down"></i></div>
          <el-dropdown-menu slot="dropdown">
            <el-dropdown-item @click.native="tabsCloseCurrentHandle">Đóng tab hiện tại</el-dropdown-item>
            <el-dropdown-item @click.native="tabsCloseOtherHandle">Đóng các tab khác</el-dropdown-item>
            <el-dropdown-item @click.native="tabsCloseAllHandle">Đóng tất cả tab</el-dropdown-item>
            <el-dropdown-item @click.native="tabsRefreshCurrentHandle">Làm mới tab hiện tại</el-dropdown-item>
          </el-dropdown-menu>
        </el-dropdown>
        <el-tab-pane v-for="item in mainTabs"
                     :key="item.name" :label="item.title" :name="item.name">
          <span slot="label"><i :class="item.icon"></i> {{ item.title }} </span>
        </el-tab-pane>
      </el-tabs>
    </div>
    <!-- Vùng nội dung chính -->
    <div class="main-content">
      <keep-alive>
        <transition name="fade" mode="out-in">
          <router-view></router-view>
        </transition>
      </keep-alive>
    </div>
  </div>
</template>

<script>
import {mapState} from 'vuex'

export default {
  name: "MainContent",
  computed: {
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
    },
    ...mapState({
      themeColor: state => state.app.themeColor,
      collapse: state => state.app.collapse,
      fullscreen: state => state.app.fullscreen,
    })
  },
  methods: {
    // tabs, đã chọn tab
    selectedTabHandle(elTab) {
      console.log(this.mainTabs)
      console.log('Đã chọn tab:', elTab)
      let tabSet = this.mainTabs.filter(item => item.name === elTab.name)
      if (tabSet.length >= 1) {
        let tab = tabSet[0]
        this.$router.push({name: tab.name, params: tab.params})
      }
    },
    // tabs, xóa tab
    removeTabHandle(tabName) {
      this.mainTabs = this.mainTabs.filter(item => item.name !== tabName)
      if (this.mainTabs.length >= 1) {
        // Tab đang chọn bị xóa
        if (tabName === this.mainTabsActiveName) {
          this.$router.push({name: this.mainTabs[this.mainTabs.length - 1].name}, () => {
            this.mainTabsActiveName = this.$route.name
          })
        }
      } else {
        this.$router.push("/")
      }
    },
    // tabs, đóng tab hiện tại
    tabsCloseCurrentHandle() {
      this.removeTabHandle(this.mainTabsActiveName)
    },
    // tabs, đóng các tab khác
    tabsCloseOtherHandle() {
      this.mainTabs = this.mainTabs.filter(item => item.name === this.mainTabsActiveName)
    },
    // tabs, đóng tất cả
    tabsCloseAllHandle() {
      this.mainTabs = []
      this.$router.push("/")
    },
    // tabs, làm mới tab hiện tại
    tabsRefreshCurrentHandle() {
      let tempTabName = this.mainTabsActiveName
      this.removeTabHandle(tempTabName)
      this.$nextTick(() => {
        this.$router.push({name: tempTabName})
      })
    }
  }
}
</script>

<style lang="scss" scoped>
.main-container {
  padding: 0 5px 5px;
  position: absolute;
  top: 60px;
  left: 1px;
  right: 1px;
  bottom: 0;
  //background: rgba(67, 69, 70, 0.1);

  .tabs {
    position: fixed;
    top: 60px;
    right: 50px;
    padding-left: 0px;
    padding-right: 2px;
    z-index: 1020;
    height: 40px;
    line-height: 40px;
    font-size: 14px;
    background: rgb(255, 253, 255);
    border-color: rgba(200, 206, 206, 0.5);
    // border-left-width: 1px;
    // border-left-style: solid;
    border-bottom-width: 1px;
    border-bottom-style: solid;
  }

  .tabs-tools {
    position: fixed;
    top: 60px;
    right: 0;
    z-index: 1020;
    height: 40px;
    // padding: 0 10px;
    font-size: 14px;
    line-height: 40px;
    cursor: pointer;
    border-color: rgba(200, 206, 206, 0.5);
    border-left-width: 1px;
    border-left-style: solid;
    border-bottom-width: 1px;
    border-bottom-style: solid;
    background: rgba(255, 255, 255, 1);
  }

  .tabs-tools:hover {
     background: rgba(200, 206, 206, 1);
  }

  .main-content {
    position: absolute;
    top: 45px;
    left: 5px;
    right: 5px;
    bottom: 5px;
    padding: 5px;
  }
}

.position-left {
  left: 200px;
}

.position-collapse-left {
  left: 65px;
}
</style>
