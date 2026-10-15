<template>
  <div>
    <reason-dialog ref="addReasonDialog" @addReason="addReason"></reason-dialog>
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
        prop="name"
        label="name"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="description"
        label="description"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="url"
        label="url"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="webMaster"
        label="webMaster"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="email"
        label="email"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="verified"
        label="verified"
      >
        <template slot-scope="scope">
          <el-tag size="medium" v-if="scope.row.verified">{{ scope.row.verified }}</el-tag>
          <el-tag size="medium" v-else type="danger">{{ scope.row.verified }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column
        prop="reason"
        label="reason"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="applyTimeStr"
        label="Thời gian yêu cầu"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        fixed="right"
        label="Thao tác"
        width="150">
        <template slot-scope="scope">
          <el-link type="primary" @click="accept(scope.row)">Duyệt</el-link>
          <el-link type="info" @click="reject(scope.row)">Từ chối</el-link>
          <el-link type="danger" @click="onItemDeleteClick(scope.row)">Xóa</el-link>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import ReasonDialog from "@/views/LinkExchange/ReasonDialog.vue"
import * as utils from '@/utils/dateTime'
import AddLinkDialog from "@/views/Link/AddDialog.vue";


export default {
  name: "LinkExchangeList",
  components: {
    AddLinkDialog,
    ReasonDialog,
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
      this.$api.linkExchange.getAll()
        .then(res => {
          this.data = res.data
          this.data.forEach(item => {
            item.applyTimeStr = utils.dateTimeBeautify(item.applyTime)
          })
        })
        .catch(res => this.$message.error(`Lỗi khi lấy danh sách: ${res.message}`))
    },
    onItemDeleteClick(item) {
      this.$confirm('Thao tác này sẽ xóa yêu cầu này, bạn có muốn tiếp tục không?', 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(() => {
        this.$api.linkExchange.deleteItem(item.id)
          .then(res => this.$message.success(`Xóa thành công. ${res.message}`))
          .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          .finally(() => this.loadData())
      }).catch(() => this.$message('Đã hủy xóa'))
    },
    addReason(id, action, reason) {
      console.log(id, action, reason)
      let promise = undefined
      if (action === 'accept') {
        promise = this.$api.linkExchange.accept(id, reason)
      } else if (action === 'reject') {
        promise = this.$api.linkExchange.reject(id, reason)
      }

      if (promise) {
        promise.then(res => this.$message.success('Thao tác thành công'))
          .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          .finally(() => this.loadData())
      }
    },
    accept(item) {
      this.$refs.addReasonDialog.show(item.id, 'accept')
    },
    reject(item) {
      this.$refs.addReasonDialog.show(item.id, 'reject')
    }
  }
}
</script>

<style scoped>

</style>
