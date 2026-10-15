<template>
  <div>
    <add-config-dialog ref="addConfigDialog" @onAddSucceed="onAddSucceed"
                       @onUpdateSucceed="onUpdateSucceed"></add-config-dialog>
    <el-table
      ref="table"
      :data="data"
      stripe
      style="width: 100%"
      :default-sort="{prop: 'time',order:'descending'}">
      <el-table-column
        type="selection"
        width="30"/>
      <el-table-column
        prop="id"
        label="id"
        width="100"/>
      <el-table-column
        prop="key"
        label="key"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="value"
        label="value"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="description"
        label="Mô tả"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        align="right">
        <template slot="header" slot-scope="scope">
          <el-button size="mini" @click="handleAdd">Thêm mục cấu hình</el-button>
        </template>
        <template slot-scope="scope">
          <el-link type="info" @click="onItemEditClick(scope.row)">Chỉnh sửa</el-link>
          <el-link type="danger" @click="onItemDeleteClick(scope.row)">Xóa</el-link>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import AddConfigDialog from "@/views/Config/AddDialog"

export default {
  name: "ConfigList",
  components: {
    AddConfigDialog,
  },
  data() {
    return {
      data: []
    }
  },
  mounted() {
    this.loadData()
  },
  methods: {
    loadData() {
      this.$api.config.getAll()
        .then(res => this.data = res.data)
        .catch(res => this.$message.error(`Lỗi khi lấy danh sách cấu hình: ${res.message}`))
    },
    handleAdd() {
      this.$refs.addConfigDialog.show()
    },
    onItemEditClick(item) {
      this.$refs.addConfigDialog.edit(item)
    },
    onItemDeleteClick(item) {
      this.$confirm('Thao tác này sẽ xóa mục cấu hình này, bạn có muốn tiếp tục không?', 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(() => {
        this.$api.config.deleteItem(item.key)
          .then(res => this.$message.success(`Xóa thành công. ${res.message}`))
          .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          .finally(() => this.loadData())
      }).catch(() => this.$message('Đã hủy xóa'))
    },
    onAddSucceed() {
      this.$message.success('Thêm thành công')
      this.loadData()
    },
    onUpdateSucceed() {
      this.$message.success('Lưu thành công')
      this.loadData()
    },
  }
}
</script>

<style scoped>

</style>
