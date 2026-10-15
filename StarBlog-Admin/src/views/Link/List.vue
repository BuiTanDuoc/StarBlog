<template>
  <div>
    <add-link-dialog ref="addDialog" @onAddSucceed="onAddSucceed"
                     @onUpdateSucceed="onUpdateSucceed"/>
    <el-table
      ref="table"
      :data="data"
      height="730"
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
        prop="name"
        label="name"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="url"
        label="url"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="description"
        label="description"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="visible"
        label="visible"
      >
        <template slot-scope="scope">
          <el-tag size="medium" v-if="scope.row.visible">{{ scope.row.visible }}</el-tag>
          <el-tag size="medium" v-else type="danger">{{ scope.row.visible }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column
        align="right">
        <template slot="header" slot-scope="scope">
          <el-button @click="handleAdd" size="mini">Thêm liên kết</el-button>
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
import AddLinkDialog from "@/views/Link/AddDialog"

export default {
  name: "LinkList",
  components: {
    AddLinkDialog,
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
      this.$api.link.getAll()
        .then(res => this.data = res.data)
        .catch(res => this.$message.error(`Lỗi khi lấy danh sách: ${res.message}`))
    },
    handleAdd() {
      this.$refs.addDialog.add()
    },
    onItemEditClick(item) {
      this.$refs.addDialog.edit(item)
    },
    onItemDeleteClick(item) {
      this.$confirm('Thao tác này sẽ xóa liên kết này, bạn có muốn tiếp tục không?', 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(() => {
        this.$api.link.deleteItem(item.id)
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
