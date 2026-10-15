<template>
  <el-result icon="info" title="Bài viết được ghim" subTitle="Thông tin bài viết đang được ghim">
    <template slot="extra">
      <p>Danh mục: {{ data.category.name }}</p>
      <p>ID: {{ data.id }}</p>
      <el-link type="primary" :href="`${baseUrl}/Blog/Post/${data.id}`" target="_blank">Tiêu đề: {{ data.title }}</el-link>
    </template>
  </el-result>
</template>

<script>
import {baseUrl} from "@/utils/global";

export default {
  name: "TopPost",
  data() {
    return {
      data: null,
      baseUrl: baseUrl
    }
  },
  mounted() {
    this.loadData()
  },
  methods: {
    loadData() {
      this.$api.blog.top()
        .then(res => this.data = res.data)
    },
  }
}
</script>

<style scoped>

</style>
