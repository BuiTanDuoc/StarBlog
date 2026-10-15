<template>
  <div class="categories-tree-container">
    <!-- Bảng bên trái -->
    <div class="left-panel" :style="{ width: leftPanelWidth + '%' }">
      <CategoryTreePanel
        :tree-data="treeData"
        :loading="loading"
        :selected-category="selectedCategory"
        @node-click="handleNodeClick"
        @menu-command="handleMenuCommand"
        @add-category="handleAdd"
      />
    </div>

    <!-- Đường phân cách -->
    <ResizableDivider
      :is-resizing="isResizing"
      @start-resize="startResize"
    />

    <!-- Bảng bên phải -->
    <div class="right-panel" :style="{ width: (100 - leftPanelWidth) + '%' }">
      <CategoryDetailPanel
        :selected-category="selectedCategory"
        :category-posts="categoryPosts"
        :posts-loading="postsLoading"
        @create-post="createPost"
        @add-subcategory="addSubCategory"
        @edit-category="editCategory"
        @refresh-posts="loadCategoryPosts"
        @view-post="viewPost"
        @edit-post="editPost"
        @delete-post="deletePost"
      />
    </div>

    <!-- Hộp thoại thêm danh mục -->
    <AddCategoryDialog
      ref="addDialog"
      @add-succeed="onAddSucceed"
      @update-succeed="onUpdateSucceed"
    />
  </div>
</template>

<script>
import AddCategoryDialog from "@/views/Category/AddDialog.vue";
import CategoryTreePanel from "./components/CategoryTreePanel.vue";
import CategoryDetailPanel from "./components/CategoryDetailPanel.vue";
import ResizableDivider from "./components/ResizableDivider.vue";

export default {
  name: "CategoriesTree",
  components: {
    AddCategoryDialog,
    CategoryTreePanel,
    CategoryDetailPanel,
    ResizableDivider
  },
  data() {
    return {
      loading: false,
      treeData: [],
      selectedCategory: null, // Danh mục đang chọn
      categoryPosts: [], // Danh sách bài viết của danh mục hiện tại
      postsLoading: false, // Trạng thái tải bài viết
      leftPanelWidth: 35, // Phần trăm chiều rộng bảng bên trái
      isResizing: false // Có đang thay đổi kích thước hay không
    }
  },
  watch: {
    // Gỡ watch của searchText vì chức năng tìm kiếm đã được chuyển sang component con
  },
  mounted() {
    this.loadData()
    // Thêm bộ lắng nghe sự kiện chuột để thay đổi kích thước bảng
    document.addEventListener('mousemove', this.handleResize)
    document.addEventListener('mouseup', this.stopResize)
  },
  beforeDestroy() {
    // Dọn dẹp bộ lắng nghe sự kiện
    document.removeEventListener('mousemove', this.handleResize)
    document.removeEventListener('mouseup', this.stopResize)
  },
  methods: {
    // Xử lý lệnh menu
    handleMenuCommand(command) {
      const { action, data } = command
      switch (action) {
        case 'addSubCategory':
          this.addSubCategory(data)
          break
        case 'viewPosts':
          this.viewCategoryPosts(data)
          break
        case 'editCategory':
          this.editCategory(data)
          break
        case 'deleteCategory':
          this.deleteCategory(data)
          break
      }
    },

    // Xử lý sự kiện nhấp vào nút
    handleNodeClick(data) {
      this.selectCategory(data)
    },

    // Chọn danh mục
    selectCategory(category) {
      this.selectedCategory = category
      this.loadCategoryPosts()
    },

    // Tải bài viết của danh mục
    async loadCategoryPosts() {
      if (!this.selectedCategory) return

      this.postsLoading = true
      try {
        const res = await this.$api.category.getPosts(
          this.selectedCategory.id, // categoryId
          1, // page
          50 // pageSize
        )
        console.log('Tải bài viết của danh mục thành công:', res.data);
        this.categoryPosts = res.data || []
      } catch (error) {
        console.error('Tải bài viết của danh mục thất bại:', error)
        this.$message.error('Tải bài viết của danh mục thất bại')
        this.categoryPosts = []
      } finally {
        this.postsLoading = false
      }
    },

    // Thay đổi kích thước bảng
    startResize(e) {
      this.isResizing = true
      e.preventDefault()
    },

    handleResize(e) {
      if (!this.isResizing) return

      const container = this.$el
      const containerRect = container.getBoundingClientRect()
      const newWidth = ((e.clientX - containerRect.left) / containerRect.width) * 100

      // Giới hạn phạm vi chiều rộng trong khoảng 20%-60%
      if (newWidth >= 20 && newWidth <= 60) {
        this.leftPanelWidth = newWidth
      }
    },

    stopResize() {
      this.isResizing = false
    },

    // Tải dữ liệu
    async loadData() {
      this.loading = true
      try {
        const res = await this.$api.category.getNodes()
        this.treeData = res.data || []
      } catch (error) {
        console.error(error)
        this.$message.error(error.message || 'Tải dữ liệu danh mục thất bại')
      } finally {
        this.loading = false
      }
    },

    // Lấy key của tất cả các nút
    getAllNodeKeys(nodes) {
      let keys = []
      nodes.forEach(node => {
        keys.push(node.id)
        if (node.nodes && node.nodes.length > 0) {
          keys = keys.concat(this.getAllNodeKeys(node.nodes))
        }
      })
      return keys
    },

    // Xem bài viết của danh mục
    viewCategoryPosts(category) {
      // Chuyển đến trang chi tiết danh mục
      this.$router.push({
        path: `/category/detail/${category.id}`,
      })
    },

    // Xem chi tiết bài viết
    viewPost(post) {
      // Chuyển đến trang chỉnh sửa bài viết
      this.$router.push({
        path: `/post/edit/${post.id}`,
      })
    },

    // Sửa bài viết
    editPost(post) {
      this.$router.push({
        path: `/post/edit/${post.id}`,
      })
    },

    // Xóa bài viết
    deletePost(post) {
      this.$confirm(`Bạn có chắc muốn xóa bài viết "${post.title}" không?`, 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(async () => {
        try {
          await this.$api.blogPost.deleteItem(post.id)
          this.$message.success('Xóa thành công')
          this.loadCategoryPosts() // Tải lại danh sách bài viết
        } catch (error) {
          this.$message.error(`Xóa thất bại: ${error.message}`)
        }
      }).catch(() => {
        this.$message('Đã hủy xóa')
      })
    },

    // Tạo bài viết
    createPost() {
      this.$router.push({
        path: '/blog/add',
        query: { categoryId: this.selectedCategory && this.selectedCategory.id }
      })
    },

    // Thêm danh mục
    handleAdd() {
      this.$refs.addDialog.add()
    },

    // Thêm danh mục con
    addSubCategory(parentCategory) {
      // Gọi phương thức addSubCategory của hộp thoại, truyền vào thông tin danh mục cha
      this.$refs.addDialog.addSubCategory({
        id: parentCategory.id,
        name: parentCategory.text
      })
    },

    // Sửa danh mục
    editCategory(category) {
      // Chuyển dữ liệu cây sang định dạng cần thiết cho việc chỉnh sửa
      const editData = {
        id: category.id,
        name: category.text,
        // Ở đây có thể cần gọi API thực tế để lấy thêm thông tin chi tiết
      }
      this.$refs.addDialog.edit(editData)
    },

    // Xóa danh mục
    deleteCategory(category) {
      this.$confirm(`Thao tác này sẽ xóa danh mục"${category.text}", bạn có muốn tiếp tục không?`, 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(async () => {
        try {
          const res = await this.$api.category.deleteItem(category.id)
          this.$message.success(`Xóa thành công. ${res.message || ''}`)
          this.loadData()
          // Nếu danh mục bị xóa là danh mục đang chọn thì xóa lựa chọn
          if (this.selectedCategory && this.selectedCategory.id === category.id) {
            this.selectedCategory = null
            this.categoryPosts = []
          }
        } catch (error) {
          this.$message.error(`Thao tác thất bại. ${error.message}`)
        }
      }).catch(() => {
        this.$message('Đã hủy xóa')
      })
    },

    // Callback khi thêm thành công
    onAddSucceed() {
      this.$message.success('Thêm thành công')
      this.loadData()
    },

    // Callback khi cập nhật thành công
    onUpdateSucceed() {
      this.$message.success('Lưu thành công')
      this.loadData()
      // Nếu danh mục được cập nhật là danh mục đang chọn thì tải lại bài viết
      if (this.selectedCategory) {
        this.loadCategoryPosts()
      }
    }
  }
}
</script>

<style scoped>
.categories-tree-container {
  display: flex;
  height: 100vh;
  background: #f5f5f5;
}

/* Kiểu dáng bảng bên trái */
.left-panel {
  background: white;
  border-right: 1px solid #e4e7ed;
  display: flex;
  flex-direction: column;
  min-width: 250px;
  max-width: 500px;
}

/* Kiểu dáng bảng bên phải */
.right-panel {
  background: white;
  display: flex;
  flex-direction: column;
  min-width: 400px;
}

/* Thiết kế responsive */
@media (max-width: 768px) {
  .categories-tree-container {
    flex-direction: column;
    height: auto;
  }

  .left-panel,
  .right-panel {
    width: 100% !important;
    min-width: auto;
  }
}
</style>
