<template>
  <el-row v-if="featuredPhotos.length>0" :gutter="10">
    <el-col :span="4" v-for="fp in featuredPhotos" :key="fp.id">
      <el-card :body-style="{ padding: '0px' }">
        <el-image :src="fp.photo.url" class="image" alt="" :preview-src-list="[fp.photo.url]"></el-image>
        <div style="padding: 14px;">
          <div>Tiêu đề ảnh: {{ fp.photo.title }}</div>
          <div>Địa điểm chụp: {{ fp.photo.location }}</div>
          <div style="margin-top: 3px;">
            <time class="time">{{ fp.photo.dateTimeStr }}</time>
          </div>
          <div class="bottom clearfix" style="margin-top: 3px;">
            <el-button-group>
              <el-button type="info" icon="el-icon-warning-outline" plain @click="notImpl"></el-button>
              <el-button type="primary" icon="el-icon-edit" plain @click="notImpl"></el-button>
              <el-button type="danger" icon="el-icon-delete" plain @click="deleteItem(fp)"></el-button>
            </el-button-group>
          </div>
        </div>
      </el-card>
    </el-col>
  </el-row>
  <el-empty v-else description="Ảnh đề xuất"></el-empty>
</template>

<script>
import {baseUrl} from "@/utils/global"

export default {
  name: "FeaturedPhotos",
  components: {},
  data() {
    return {
      featuredPhotos: [],
    }
  },
  mounted() {
    this.loadData()
  },
  methods: {
    loadData() {
      this.$api.featuredPhoto.getAll().then(res => {
        console.log(res)
        this.featuredPhotos = res.data
        this.featuredPhotos.forEach(item => {
          item.photo.url = `${baseUrl}/media/photography/${item.photo.id}.jpg`
          let dt = new Date(item.photo.createTime)
          item.photo.dateTimeStr = `${dt.toLocaleDateString()} ${dt.toLocaleTimeString()}`
        })
        console.log('featuredPhotos', this.featuredPhotos)
      })
    },
    notImpl() {
      this.$message('Chưa triển khai đâu nhé~')
    },
    deleteItem(featuredPhoto) {
      this.$confirm('Thao tác này sẽ xóa ảnh đề xuất này, bạn có muốn tiếp tục không?', 'Thông báo', {type: 'warning'})
        .then(() => {
          this.$api.featuredPhoto.deleteItem(featuredPhoto.id)
            .then(res => this.$message.success(`Xóa thành công. ${res.message}`))
            .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
            .finally(() => this.loadData())
        })
        .catch(() => this.$message('Đã hủy xóa'))
    }
  }
}
</script>

<style scoped>

</style>
