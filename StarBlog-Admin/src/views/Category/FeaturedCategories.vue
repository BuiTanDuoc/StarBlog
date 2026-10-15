<template>
  <el-table
    v-loading="loading"
    :data="tableData"
    :height="600"
    stripe
    style="width: 100%;">
    <el-table-column
      prop="id"
      label="ID"
      width="50">
    </el-table-column>
    <el-table-column
      prop="name"
      label="Tên đề xuất"
      width="250">
    </el-table-column>
    <el-table-column
      prop="description"
      label="Mô tả đề xuất"
      width="500"
      :show-overflow-tooltip="true">
    </el-table-column>
    <el-table-column
      prop="iconCssClass"
      label="Biểu tượng"
      width="200">
    </el-table-column>
    <el-table-column
      prop="category.id"
      label="ID danh mục"
      width="80">
    </el-table-column>
    <el-table-column
      prop="category.name"
      label="Tên danh mục">
    </el-table-column>
    <el-table-column
      align="right">
      <template slot="header" slot-scope="scope">
        <el-input
          v-model="search"
          size="mini"
          placeholder="Nhập từ khóa để tìm kiếm"/>
      </template>
      <template slot-scope="scope">
        <el-button
          size="mini"
          type="warning" plain
          @click="cancelFeatured(scope.$index, scope.row)">Bỏ đề xuất
        </el-button>
      </template>
    </el-table-column>
  </el-table>
</template>

<script>
export default {
  name: "FeaturedCategories",
  data() {
    return {
      loading: false,
      data: [],
      search: ''
    }
  },
  mounted() {
    this.loadData()
  },
  computed: {
    tableData() {
      return this.data.filter(item =>
        !this.search || item.name.toLowerCase().includes(this.search.toLowerCase()))
    }
  },
  methods: {
    loadData() {
      this.loading = true
      this.$api.featuredCategory.getAll()
        .then(res => this.data = res.data)
        .catch(res => {
          console.error(res)
          this.$message.error(res.message)
        })
        .finally(() => this.loading = false)
    },
    cancelFeatured(index, item) {
      this.$confirm('Bạn có chắc không?', 'Are you sure?', {type: 'warning'})
        .then(() => {
          this.$api.featuredCategory.deleteItem(item.id)
            .then(res => this.$message.success(`Thao tác thành công. ${res.message}`))
            .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
            .finally(() => this.loadData())
        })
        .catch(() => this.$message('Hủy thao tác'))
    }
  }
}
</script>

<style scoped>

</style>
