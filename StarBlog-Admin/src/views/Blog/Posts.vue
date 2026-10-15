<template>
  <el-container>
    <el-header height="30px">
      <el-row type="flex" justify="space-between">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-input v-model="search" placeholder="Vui lòng nhập từ khóa" prefix-icon="el-icon-search" clearable
              @input="handleSearchInput" @clear="handleSearchClear">
            </el-input>
          </el-col>
          <el-col :span="8">
            <el-cascader class="w-100" :options="categoriesTree" v-model="currentCategoryId" filterable clearable
              placeholder="Lọc danh mục" :props="{
                checkStrictly: true,
                expandTrigger: 'hover',
                emitPath: false,
              }" @change="handleCategoryChange">
            </el-cascader>
          </el-col>
          <el-col :span="3">
            <el-select v-model="currentStatus" clearable filterable placeholder="Đánh dấu bài viết" @change="handleStatusChange">
              <el-option v-for="item in statusList" :key="item" :label="item" :value="item" />
            </el-select>
          </el-col>
          <el-col :span="3">
            <el-select v-model="currentIsPublish" clearable placeholder="Trạng thái bài viết" @change="handlePublishStatusChange">
              <el-option label="Tất cả" :value="null" />
              <el-option label="Đã xuất bản" :value="true" />
              <el-option label="Bản nháp" :value="false" />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-button @click="handleClearFilters" type="info" plain>Xóa bộ lọc</el-button>
          </el-col>
        </el-row>
        <div>
          <el-button @click="addPost" type="primary">Thêm bài viết</el-button>
          <el-dropdown @command="handleBatchCommand" :disabled="!hasSelection">
            <el-button type="warning" :disabled="!hasSelection">
              Thao tác hàng loạt<i class="el-icon-arrow-down el-icon--right"></i>
            </el-button>
            <el-dropdown-menu slot="dropdown">
              <el-dropdown-item command="batchDelete">Xóa hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchMoveCategory">Chuyển danh mục hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchSetFeatured">Thiết lập đề xuất hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchCancelFeatured">Bỏ đề xuất hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchSetTop">Thiết lập ghim hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchPublish">Xuất bản hàng loạt</el-dropdown-item>
              <el-dropdown-item command="batchUnpublish">Đặt làm bản nháp hàng loạt</el-dropdown-item>
            </el-dropdown-menu>
          </el-dropdown>
          <el-button @click="toggleSelection()" :disabled="!hasSelection">Bỏ chọn</el-button>
        </div>
      </el-row>
    </el-header>
    <el-main>
      <!-- Skeleton khi tải -->
      <div v-if="loading && posts.length === 0" class="skeleton-container">
        <el-skeleton :rows="8" animated />
      </div>

      <!-- Bảng -->
      <el-table v-else v-loading="loading" ref="table" :data="posts" stripe style="width: 100%"
        height="calc(100vh - 280px)" @selection-change="handleSelectionChange"
        :default-sort="{ prop: 'lastUpdateTime', order: 'descending' }" empty-text="Chưa có dữ liệu">
        <el-table-column type="selection" width="50" fixed="left" />
        <el-table-column prop="id" label="ID" width="80" :show-overflow-tooltip="true" />
        <el-table-column prop="isPublish" label="Trạng thái" width="150" fixed="left">
          <template v-slot="scope">
            <el-switch v-model="scope.row.isPublish" active-text="Đã xuất bản" inactive-text="Bản nháp"
              @change="handlePublishToggle(scope.row)" :loading="scope.row.statusLoading">
            </el-switch>
          </template>
        </el-table-column>
        <el-table-column prop="title" label="Tiêu đề" min-width="300" sortable :show-overflow-tooltip="true">
          <template v-slot="scope">
            <div class="title-cell">
              <el-link :href="`${baseUrl}/Blog/Post/${scope.row.id}`" target="_blank" class="post-title">
                {{ scope.row.title }}
              </el-link>
              <div class="post-summary" v-if="scope.row.summary">
                {{ scope.row.summary }}
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="Đánh dấu" width="120">
          <template v-slot="scope">
            <el-tag v-if="scope.row.status" size="small" type="info">{{ scope.row.status }}</el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="viewCount" label="Lượt đọc" width="100" sortable>
          <template v-slot="scope">
            <span>{{ scope.row.viewCount || 0 }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="creationTime" label="Thời gian tạo" width="180" sortable>
          <template v-slot="scope">
            <div class="time-cell">
              <i class="el-icon-time"></i>
              <span>{{ formatDateTime(scope.row.creationTime) }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="category.name" label="Danh mục" width="150" :show-overflow-tooltip="true">
          <template v-slot="scope">
            <el-tag v-if="scope.row.category" size="small">{{ scope.row.category.name }}</el-tag>
            <span v-else>Chưa phân loại</span>
          </template>
        </el-table-column>
        <el-table-column align="center" label="Thao tác" width="240" fixed="right">
          <template v-slot="scope">
            <div class="action-buttons">
              <el-button type="text" size="mini" @click="onItemEditClick(scope.row)">
                <i class="el-icon-edit"></i> Chỉnh sửa
              </el-button>
              <el-button type="text" size="mini" @click="copyPostLink(scope.row)">
                <i class="el-icon-link"></i> Sao chép liên kết
              </el-button>
              <el-dropdown @command="cmd => onItemDropdownClick(scope.row, cmd)" size="mini">
                <el-button type="text" size="mini">
                  Thêm<i class="el-icon-arrow-down el-icon--right"></i>
                </el-button>
                <el-dropdown-menu slot="dropdown">
                  <el-dropdown-item command="setFeatured" :disabled="scope.row.isFeatured">Đặt đề xuất</el-dropdown-item>
                  <el-dropdown-item command="cancelFeatured" :disabled="!scope.row.isFeatured">Bỏ đề xuất</el-dropdown-item>
                  <el-dropdown-item command="setTop" :disabled="scope.row.isTop">Đặt ghim</el-dropdown-item>
                  <el-dropdown-item command="cancelTop" :disabled="!scope.row.isTop">Bỏ ghim</el-dropdown-item>
                  <el-dropdown-item command="moveToCategory" divided>Chuyển đến danh mục</el-dropdown-item>
                  <el-dropdown-item command="delete">
                    <span style="color: #f56c6c;">Xóa bài viết</span>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </el-dropdown>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <!-- Phân trang -->
      <el-pagination class="py-3 text-center" @size-change="handlePageSizeChange"
        @current-change="handleCurrentPageChange" :current-page="currentPage" :page-sizes="[10, 20, 40, 60, 80, 100]"
        :page-size="pageSize" background layout="total, sizes, prev, pager, next, jumper" :total="totalCount">
      </el-pagination>
    </el-main>

    <!-- Hộp thoại chuyển danh mục hàng loạt -->
    <el-dialog title="Chuyển danh mục hàng loạt" :visible.sync="batchMoveCategoryVisible" width="500px">
      <el-form>
        <el-form-item label="Danh mục đích">
          <el-cascader v-model="targetCategoryId" :options="categoriesTree" placeholder="Vui lòng chọn danh mục đích" :props="{
            checkStrictly: true,
            expandTrigger: 'hover',
            emitPath: false,
          }" style="width: 100%">
          </el-cascader>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click="batchMoveCategoryVisible = false">Hủy</el-button>
        <el-button type="primary" @click="confirmBatchMoveCategory" :loading="batchOperationLoading">Xác nhận</el-button>
      </div>
    </el-dialog>

    <!-- Hộp thoại chuyển danh mục cho một bài viết -->
    <el-dialog title="Chuyển đến danh mục" :visible.sync="moveCategoryVisible" width="500px">
      <el-form>
        <el-form-item label="Tiêu đề bài viết">
          <span>{{ currentMovingPost ? currentMovingPost.title : '' }}</span>
        </el-form-item>
        <el-form-item label="Danh mục hiện tại">
          <span>{{ currentMovingPost && currentMovingPost.category ? currentMovingPost.category.name : 'Chưa phân loại' }}</span>
        </el-form-item>
        <el-form-item label="Danh mục đích">
          <el-cascader v-model="singleTargetCategoryId" :options="categoriesTree" placeholder="Vui lòng chọn danh mục đích" :props="{
            checkStrictly: true,
            expandTrigger: 'hover',
            emitPath: false,
          }" style="width: 100%">
          </el-cascader>
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button @click="moveCategoryVisible = false">Hủy</el-button>
        <el-button type="primary" @click="confirmSingleMoveCategory" :loading="batchOperationLoading">Xác nhận</el-button>
      </div>
    </el-dialog>
  </el-container>
</template>

<script>
import * as utils from '@/utils/dateTime'
import { baseUrl } from "@/utils/global";

export default {
  name: 'Posts',
  data() {
    return {
      baseUrl: baseUrl,
      loading: false,
      currentPage: 1,
      pageSize: 20,
      totalCount: 1000,
      search: '',
      sortBy: null,
      posts: [],
      categoriesTree: [],
      statusList: [],
      currentCategoryId: 0,
      currentStatus: null,
      currentIsPublish: null,
      selectedPosts: [],
      hasSelection: false,
      searchTimer: null, // Bộ đếm thời gian debounce cho tìm kiếm
      batchMoveCategoryVisible: false,
      targetCategoryId: null,
      batchOperationLoading: false,
      // Liên quan đến chuyển danh mục cho một bài viết
      moveCategoryVisible: false,
      currentMovingPost: null,
      singleTargetCategoryId: null
    }
  },
  mounted() {
    // Tải danh mục
    this.loadCategories()
    // Tải bài viết blog
    this.loadBlogPosts()
    // Tải danh sách trạng thái bài viết
    this.loadStatusList()
  },
  methods: {
    /**
     * Tải cây danh mục
     */
    loadCategories() {
      const mapNodes = (nodes) => {
        let items = []
        if (!nodes) return null
        for (const node of nodes) {
          items.push({
            label: `${node.text} (${node.tags[0]})`,
            value: node.id,
            children: mapNodes(node.nodes)
          })
        }
        return items
      }

      this.$api.category.getNodes()
        .then(res => this.categoriesTree = mapNodes(res.data))
        .catch(res => this.$message.error(`Lỗi khi tải danh sách danh mục: ${res.message}`))
    },

    /**
     * Tải danh sách bài viết blog
     */
    loadBlogPosts() {
      this.loading = true
      this.$api.blogPost.getList(
        this.currentIsPublish, this.currentStatus,
        this.currentCategoryId, this.search, this.sortBy,
        this.currentPage, this.pageSize
      ).then(res => {
        console.log(res)
        this.totalCount = res.pagination.totalItemCount
        this.posts = res.data.map(item => ({
          ...item,
          statusLoading: false // Thêm cờ tải cho chuyển đổi trạng thái
        }))
      }).catch(res => this.$message.error(`Lỗi khi lấy danh sách bài viết: ${res.message}`))
        .finally(() => this.loading = false)
    },

    /**
     * Tải danh sách trạng thái bài viết
     */
    loadStatusList() {
      this.$api.blog.getStatusList().then(res => {
        this.statusList = res.data
      }).catch(res => this.$message.error(`Lỗi khi tải danh sách trạng thái: ${res.message}`))
    },

    /**
     * Định dạng hiển thị ngày giờ
     */
    formatDateTime(dateTime) {
      if (!dateTime) return '-'
      return utils.dateTimeBeautify(dateTime)
    },

    /**
     * Xử lý nhập tìm kiếm (debounce)
     */
    handleSearchInput() {
      if (this.searchTimer) {
        clearTimeout(this.searchTimer)
      }
      this.searchTimer = setTimeout(() => {
        this.currentPage = 1
        this.loadBlogPosts()
      }, 500)
    },

    /**
     * Xử lý khi xóa tìm kiếm
     */
    handleSearchClear() {
      this.search = ''
      this.currentPage = 1
      this.loadBlogPosts()
    },

    /**
     * Xử lý khi danh mục thay đổi
     */
    handleCategoryChange() {
      this.currentPage = 1
      this.loadBlogPosts()
    },

    /**
     * Xử lý khi trạng thái thay đổi
     */
    handleStatusChange() {
      this.currentPage = 1
      this.loadBlogPosts()
    },

    /**
     * Xử lý khi trạng thái xuất bản thay đổi
     */
    handlePublishStatusChange() {
      this.currentPage = 1
      this.loadBlogPosts()
    },

    /**
     * Xóa tất cả điều kiện lọc
     */
    handleClearFilters() {
      this.search = ''
      this.currentCategoryId = 0
      this.currentStatus = null
      this.currentIsPublish = null
      this.currentPage = 1
      this.loadBlogPosts()
      this.$message.success('Đã xóa tất cả điều kiện lọc')
    },

    /**
     * Chuyển đổi trạng thái xuất bản
     */
    handlePublishToggle(post) {
      const targetStatus = post.isPublish
      post.statusLoading = true

      // Ở đây cần điều chỉnh theo API thực tế
      const apiCall = targetStatus ?
        this.$api.blogPost.publish(post.id) :
        this.$api.blogPost.unpublish(post.id)

      apiCall.then(() => {
        this.$message.success(`Bài viết đã ${targetStatus ? 'được xuất bản' : 'chuyển sang bản nháp'}`)
      }).catch(err => {
        post.isPublish = !targetStatus // Khôi phục trạng thái ban đầu
        this.$message.error(`Thao tác thất bại: ${err.message}`)
      }).finally(() => {
        post.statusLoading = false
      })
    },

    /**
     * Sao chép liên kết bài viết
     */
    copyPostLink(post) {
      const link = `${this.baseUrl}/Blog/Post/${post.id}`
      if (navigator.clipboard) {
        navigator.clipboard.writeText(link).then(() => {
          this.$message.success('Đã sao chép liên kết vào clipboard')
        })
      } else {
        // Tương thích trình duyệt cũ
        const textArea = document.createElement('textarea')
        textArea.value = link
        document.body.appendChild(textArea)
        textArea.select()
        document.execCommand('copy')
        document.body.removeChild(textArea)
        this.$message.success('Đã sao chép liên kết vào clipboard')
      }
    },

    /**
     * Xử lý lệnh thao tác hàng loạt
     */
    handleBatchCommand(command) {
      if (this.selectedPosts.length === 0) {
        this.$message.warning('Vui lòng chọn bài viết cần thao tác trước')
        return
      }

      switch (command) {
        case 'batchDelete':
          this.handleBatchDelete()
          break
        case 'batchMoveCategory':
          this.batchMoveCategoryVisible = true
          break
        case 'batchSetFeatured':
          this.handleBatchSetFeatured()
          break
        case 'batchCancelFeatured':
          this.handleBatchCancelFeatured()
          break
        case 'batchSetTop':
          this.handleBatchSetTop()
          break
        case 'batchPublish':
          this.handleBatchPublish()
          break
        case 'batchUnpublish':
          this.handleBatchUnpublish()
          break
      }
    },

    /**
     * Xóa bài viết hàng loạt
     */
    handleBatchDelete() {
      this.$confirm(`Bạn có chắc muốn xóa ${this.selectedPosts.length} bài viết đã chọn? Thao tác này không thể hoàn tác!`, 'Xác nhận xóa hàng loạt', {
        confirmButtonText: 'Xác nhận xóa',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(() => {
        this.batchOperationLoading = true
        const deletePromises = this.selectedPosts.map(post =>
          this.$api.blogPost.deleteItem(post.id)
        )

        Promise.all(deletePromises)
          .then(() => {
            this.$message.success(`Đã xóa thành công ${this.selectedPosts.length} bài viết`)
            this.loadBlogPosts()
            this.toggleSelection()
          })
          .catch(err => {
            this.$message.error(`Xóa hàng loạt thất bại: ${err.message}`)
          })
          .finally(() => {
            this.batchOperationLoading = false
          })
      })
    },

    /**
     * Xác nhận chuyển danh mục hàng loạt
     */
    confirmBatchMoveCategory() {
      if (!this.targetCategoryId) {
        this.$message.warning('Vui lòng chọn danh mục đích')
        return
      }

      this.batchOperationLoading = true
      const movePromises = this.selectedPosts.map(post =>
        this.$api.blogPost.update({
          ...post,
          categoryId: this.targetCategoryId
        })
      )

      Promise.all(movePromises)
        .then(() => {
          this.$message.success(`Đã chuyển thành công ${this.selectedPosts.length} bài viết sang danh mục mới`)
          this.batchMoveCategoryVisible = false
          this.targetCategoryId = null
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Chuyển danh mục hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    /**
     * Thiết lập đề xuất hàng loạt
     */
    handleBatchSetFeatured() {
      this.batchOperationLoading = true
      const promises = this.selectedPosts.map(post =>
        this.$api.blogPost.setFeatured(post.id)
      )

      Promise.all(promises)
        .then(() => {
          this.$message.success(`Đã đặt thành công ${this.selectedPosts.length} bài viết làm đề xuất`)
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Thiết lập đề xuất hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    /**
     * Bỏ đề xuất hàng loạt
     */
    handleBatchCancelFeatured() {
      this.batchOperationLoading = true
      const promises = this.selectedPosts.map(post =>
        this.$api.blogPost.cancelFeatured(post.id)
      )

      Promise.all(promises)
        .then(() => {
          this.$message.success(`Đã hủy thành công ${this.selectedPosts.length} bài viết khỏi danh sách đề xuất`)
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Bỏ đề xuất hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    /**
     * Thiết lập ghim hàng loạt
     */
    handleBatchSetTop() {
      this.batchOperationLoading = true
      const promises = this.selectedPosts.map(post =>
        this.$api.blogPost.setTop(post.id)
      )

      Promise.all(promises)
        .then(() => {
          this.$message.success(`Đã đặt thành công ${this.selectedPosts.length} bài viết làm bài ghim`)
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Thiết lập ghim hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    /**
     * Xuất bản bài viết hàng loạt
     */
    handleBatchPublish() {
      this.batchOperationLoading = true
      const promises = this.selectedPosts
        .filter(post => !post.isPublish)
        .map(post => this.$api.blogPost.update({
          ...post,
          isPublish: true
        }))

      if (promises.length === 0) {
        this.$message.info('Các bài viết đã chọn đều đang ở trạng thái xuất bản')
        return
      }

      Promise.all(promises)
        .then(() => {
          this.$message.success(`Đã xuất bản thành công ${promises.length} bài viết`)
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Xuất bản hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    /**
     * Đặt làm bản nháp hàng loạt
     */
    handleBatchUnpublish() {
      this.batchOperationLoading = true
      const promises = this.selectedPosts
        .filter(post => post.isPublish)
        .map(post => this.$api.blogPost.update({
          ...post,
          isPublish: false
        }))

      if (promises.length === 0) {
        this.$message.info('Các bài viết đã chọn đều đang là bản nháp')
        return
      }

      Promise.all(promises)
        .then(() => {
          this.$message.success(`Đã chuyển thành công ${promises.length} bài viết sang bản nháp`)
          this.loadBlogPosts()
          this.toggleSelection()
        })
        .catch(err => {
          this.$message.error(`Đặt làm bản nháp hàng loạt thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    },

    // ... existing code ...

    /**
     * Nút thêm bài viết
     */
    addPost() {
      this.$router.push('/post/new')
    },

    /**
     * Sửa bài viết
     */
    onItemEditClick(post) {
      this.$router.push(`/post/edit/${post.id}`)
    },

    /**
     * Xử lý khi nhấp vào menu thả xuống
     */
    onItemDropdownClick(post, command) {
      switch (command) {
        case 'setFeatured':
          this.$api.blogPost.setFeatured(post.id)
            .then(res => {
              this.$message.success('Đặt đề xuất thành công')
              this.loadBlogPosts()
            })
            .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          break
        case 'cancelFeatured':
          this.$api.blogPost.cancelFeatured(post.id)
            .then(res => {
              this.$message.success('Bỏ đề xuất thành công')
              this.loadBlogPosts()
            })
            .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          break
        case 'setTop':
          this.$api.blogPost.setTop(post.id)
            .then(res => {
              this.$message.success(`Đặt ghim thành công. ${res.message}`)
              this.loadBlogPosts()
            })
            .catch(res => this.$message.error(`Đặt ghim thất bại. ${res.message}`))
          break
        case 'cancelTop':
          // Cần thêm API hủy ghim
          this.$api.blogPost.cancelTop(post.id)
            .then(res => {
              this.$message.success('Bỏ ghim thành công')
              this.loadBlogPosts()
            })
            .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          break
        case 'delete':
          this.$confirm('Thao tác này sẽ xóa vĩnh viễn bài viết này, bạn có muốn tiếp tục không?', 'Thông báo', {
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy',
            type: 'warning'
          }).then(() => {
            this.$api.blogPost.deleteItem(post.id)
              .then(res => {
                this.$message.success(`Xóa thành công. ${res.message}`)
                this.loadBlogPosts()
              })
              .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          }).catch(() => this.$message('Đã hủy xóa'))
          break
        case 'moveToCategory':
          this.currentMovingPost = post
          this.singleTargetCategoryId = post.categoryId || null
          this.moveCategoryVisible = true
          break
      }
    },

    /**
     * Xử lý khi kích thước trang thay đổi
     */
    handlePageSizeChange(pageSize) {
      this.pageSize = pageSize
      this.loadBlogPosts()
    },

    /**
     * Xử lý khi trang hiện tại thay đổi
     */
    handleCurrentPageChange(page) {
      this.currentPage = page
      this.loadBlogPosts()
    },

    /**
     * Chuyển đổi trạng thái chọn
     */
    toggleSelection(rows) {
      if (rows) {
        rows.forEach(row => {
          this.$refs.table.toggleRowSelection(row);
        });
      } else {
        this.$refs.table.clearSelection();
      }
    },

    /**
     * Xử lý khi lựa chọn thay đổi
     */
    handleSelectionChange(val) {
      this.selectedPosts = val
      this.hasSelection = this.selectedPosts.length > 0
    },

    /**
     * Xác nhận chuyển danh mục cho một bài viết
     */
    confirmSingleMoveCategory() {
      if (!this.singleTargetCategoryId) {
        this.$message.warning('Vui lòng chọn danh mục đích')
        return
      }

      if (!this.currentMovingPost) {
        this.$message.error('Không tìm thấy bài viết cần chuyển')
        return
      }

      this.batchOperationLoading = true
      this.$api.blogPost.update({
        ...this.currentMovingPost,
        categoryId: this.singleTargetCategoryId
      })
        .then(() => {
          this.$message.success('Chuyển danh mục bài viết thành công')
          this.moveCategoryVisible = false
          this.currentMovingPost = null
          this.singleTargetCategoryId = null
          this.loadBlogPosts()
        })
        .catch(err => {
          this.$message.error(`Chuyển danh mục thất bại: ${err.message}`)
        })
        .finally(() => {
          this.batchOperationLoading = false
        })
    }
  }
}
</script>

<style scoped>
.el-header {
  text-align: left;
}

.skeleton-container {
  padding: 20px;
}

.title-cell {
  line-height: 1.4;
}

.post-title {
  font-weight: 500;
  color: #409EFF;
  text-decoration: none;
}

.post-title:hover {
  color: #66b1ff;
}

.post-summary {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
  line-height: 1.3;
  max-height: 32px;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.time-cell {
  display: flex;
  align-items: center;
  font-size: 13px;
}

.time-cell i {
  margin-right: 4px;
  color: #909399;
}

.action-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  justify-content: center;
  align-items: center;
}

.action-buttons .el-button {
  margin: 0;
  padding: 2px 6px;
  font-size: 12px;
}

/* Thiết kế responsive */
@media (max-width: 768px) {
  .el-header .el-row {
    flex-direction: column;
    gap: 10px;
  }

  .el-header .el-col {
    width: 100% !important;
  }

  .action-buttons {
    flex-direction: column;
    align-items: center;
  }
}

/* Tối ưu bảng */
.el-table {
  font-size: 14px;
}

.el-table .el-table__header th {
  background-color: #fafafa;
  color: #606266;
  font-weight: 500;
}

.el-table .el-table__row:hover {
  background-color: #f5f7fa;
}

/* Tối ưu kiểu phân trang */
.el-pagination {
  margin-top: 20px;
  text-align: center;
}

/* Kiểu dáng hộp thoại */
.dialog-footer {
  text-align: right;
}

.dialog-footer .el-button {
  margin-left: 10px;
}
</style>
