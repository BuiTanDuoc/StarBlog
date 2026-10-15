<template>
  <div>
    <el-table
      ref="table"
      v-loading="loading"
      :data="data"
      stripe
      class="w-100"
      :default-sort="{prop: 'time',order:'descending'}">
      <el-table-column
        type="selection"
        width="30"/>
      <el-table-column
        prop="id"
        label="id"
        :show-overflow-tooltip="true"/>
      <el-table-column
        label="Thông tin người dùng"
        :show-overflow-tooltip="true"
      >
        <template v-slot="scope">
          <el-link type="text" @click="showUserInfo(scope.row)">{{ scope.row.anonymousUser.email }}</el-link>
        </template>
      </el-table-column>
      <el-table-column
        label="Bài viết"
        :show-overflow-tooltip="true"
      >
        <template v-slot="scope">
          <el-link :href="`${baseUrl}/Blog/Post/${scope.row.post.id}`" target="_blank">{{
              scope.row.post.title
            }}
          </el-link>
        </template>
      </el-table-column>
      <el-table-column
        prop="createdTime"
        label="Thời gian bình luận"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="content"
        label="Nội dung bình luận"
        :show-overflow-tooltip="true"
      />
      <el-table-column
        prop="reason"
        label="Lý do bổ sung"
        :show-overflow-tooltip="true"/>
      <el-table-column
        prop="visible"
        label="Có hiển thị hay không"
      >
        <template v-slot="scope">
          <el-tag size="medium" v-if="scope.row.visible">Có</el-tag>
          <el-tag size="medium" v-else type="danger">Không</el-tag>
        </template>
      </el-table-column>
      <el-table-column
        prop="isNeedAudit"
        label="Cần duyệt"
      >
        <template v-slot="scope">
          <el-tag size="medium" v-if="scope.row.isNeedAudit">Có</el-tag>
          <el-tag size="medium" v-else type="danger">Không</el-tag>
        </template>
      </el-table-column>
      <el-table-column
        align="right">
        <template slot="header" slot-scope="scope">
          <el-button @click="handleRefresh" :loading="isRefreshLoading" type="primary" size="mini">Làm mới</el-button>
        </template>
        <template slot-scope="scope">
          <el-link type="primary" @click="handleAccept(scope.row)">Duyệt</el-link>
          <el-link type="danger" @click="handleReject(scope.row)">Từ chối</el-link>
        </template>
      </el-table-column>
    </el-table>
    <!-- Phân trang -->
    <el-pagination
      class="py-3 text-center"
      @size-change="handlePageSizeChange"
      @current-change="handleCurrentPageChange"
      :current-page="pagination.pageNumber"
      :page-sizes="[10, 20, 40, 60, 80, 100]"
      :page-size="pagination.pageSize"
      background
      layout="total, sizes, prev, pager, next, jumper"
      :total="pagination.totalItemCount">
    </el-pagination>
  </div>
</template>

<script>
import {dateTimeBeautify} from "@/utils/dateTime";
import {baseUrl} from '@/utils/global'


export default {
  name: "Comments",
  data() {
    return {
      data: [],
      postId: null,
      pagination: {
        pageNumber: 1,
        pageSize: 10,
      },
      loading: false,
      isRefreshLoading: false,
      baseUrl: baseUrl,
      search: '',
      sortBy: null,
    }
  },
  mounted() {
    this.loadData()
  },
  methods: {
    async loadData() {
      return new Promise((resolve, reject) => {
        this.loading = true
        this.$api.comment.getList(
          this.postId,
          this.search,
          this.sortBy,
          this.pagination.pageNumber,
          this.pagination.pageSize,
        )
          .then(res => {
            this.pagination = res.pagination
            this.data = res.data
            this.data.forEach(e => {
              e.createdTime = dateTimeBeautify(e.createdTime)
              e.updatedTime = dateTimeBeautify(e.updatedTime)
            })
            resolve(res.data)
          })
          .catch(res => {
            this.$message.error(`Lỗi khi lấy danh sách: ${res.message}`)
            reject(res)
          })
          .finally(() => this.loading = false)
      })
    },
    async handleRefresh() {
      this.isRefreshLoading = true
      await this.loadData()
      this.isRefreshLoading = false
    },
    handleAccept(item) {
      this.$prompt('Vui lòng nhập lý do', 'Duyệt bình luận - bổ sung lý do', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(({value}) => {
        this.$api.comment.accept(item.id, value)
          .then(res => {
            this.$message.success('Thao tác thành công!')
          })
          .catch(res => {
            console.error(res)
            this.$message.warning(`Thao tác thất bại! ${res.message}`)
          })
          .finally(() => this.loadData())
      }).catch(() => {
      })
    },
    handleReject(item) {
      this.$prompt('Vui lòng nhập lý do', 'Duyệt bình luận - bổ sung lý do', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(({value}) => {
        this.$api.comment.reject(item.id, value)
          .then(res => {
            this.$message.success('Thao tác thành công!')
          })
          .catch(res => {
            console.error(res)
            this.$message.warning(`Thao tác thất bại! ${res.message}`)
          })
          .finally(() => this.loadData())
      }).catch(() => {
      })
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
    handlePageSizeChange(pageSize) {
      this.pagination.pageSize = pageSize
      this.loadData()
    },
    handleCurrentPageChange(page) {
      this.pagination.pageNumber = page
      this.loadData()
    },
    showUserInfo(comment) {
      let user = comment.anonymousUser
      const h = this.$createElement;
      this.$msgbox({
        title: 'Thông tin người dùng',
        message: h('div', null, [
          h('p', null, `ID: ${user.id}`),
          h('p', null, `Tên người dùng: ${user.name}`),
          h('p', null, `Email: ${user.email}`),
          h('p', null, `Địa chỉ web: ${user.url}`),
          h('p', null, `Địa chỉ IP: ${user.ip}`),
          h('p', null, `Thời gian tạo: ${dateTimeBeautify(user.createdTime)}`),
          h('p', null, `Định danh UA: ${comment.userAgent}`),
        ]),
        showCancelButton: false,
        showConfirmButton: false,
      })
    }
  }
}
</script>

<style scoped>

</style>
