<template>
  <el-container>
    <el-header height="72px">
      <el-row type="flex" justify="space-between" align="middle">
        <div class="header-left">
          <el-button type="primary" @click="$refs.addPhotoDialog.show()">Thêm</el-button>
          <el-button :type="selectionMode ? 'warning' : ''" @click="toggleSelectionMode">
            {{ selectionMode ? 'Thoát chế độ chọn' : 'Chế độ chọn' }}
          </el-button>
          <el-dropdown :hide-on-click="false">
            <el-button type="danger" :disabled="selectedIds.length===0">
              Thao tác hàng loạt<i class="el-icon-arrow-down el-icon--right"></i>
            </el-button>
            <el-dropdown-menu slot="dropdown">
              <el-dropdown-item :disabled="selectedIds.length===0" @click.native="handleBatchDelete">Xóa hàng loạt</el-dropdown-item>
              <el-dropdown-item :disabled="selectedIds.length===0" @click.native="handleBatchSetFeatured">Đặt làm đề xuất hàng loạt</el-dropdown-item>
              <el-dropdown-item :disabled="selectedIds.length===0" @click.native="handleBatchCancelFeatured">Bỏ đề xuất hàng loạt</el-dropdown-item>
            </el-dropdown-menu>
          </el-dropdown>
        </div>
        <div class="header-right">
          <el-input placeholder="Tìm theo tiêu đề hoặc địa điểm chụp" v-model="searchKeyword" clearable size="small"
                    class="mr-2" @input="onFilterInput" @clear="applyFilters" style="width: 240px;"/>
          <el-select v-model="featuredFilter" size="small" class="mr-2" style="width: 140px;" @change="applyFilters">
            <el-option label="Tất cả" value="all"/>
            <el-option label="Đã đề xuất" value="featured"/>
            <el-option label="Chưa đề xuất" value="unfeatured"/>
          </el-select>
          <el-button size="small" @click="clearFilters">Xóa bộ lọc</el-button>
        </div>
      </el-row>
      <el-alert v-if="guideVisible" title="Mẹo sử dụng" type="info" show-icon closable class="mt-2"
                description="Nhấp vào ảnh để xem trước, nhấn Thêm ở góc trên bên phải để xem các thao tác; có thể bật chế độ chọn để xóa/đề xuất/bỏ đề xuất hàng loạt."/>
    </el-header>
    <el-main v-loading="loading">
      <div style="margin-bottom: 10px; font-size: 12px; color: #666;">
        Thông tin gỡ lỗi: loading={{loading}}, photos.length={{photos.length}}, allPhotos.length={{allPhotos.length}}
      </div>
      <template v-if="loading">
        <div class="skeleton-grid">
          <el-skeleton v-for="n in 8" :key="n" :rows="4" animated style="margin: 8px; width: 260px;"/>
        </div>
      </template>
      <template v-else>
        <Masonry v-if="photos.length > 0" :column-width="260" :gap="16">
          <photo-card v-for="photo in photos"
                      :key="photo.id"
                      :photo="photo"
                      :selection-mode="selectionMode"
                      :selected="selectedIds.includes(photo.id)"
                      @onItemDeleted="loadPhotos"
                      @onSelectChange="onCardSelectChange"
                      @onFeaturedChange="onCardFeaturedChange"
                      @onEdit="openEditDialog"
          />
        </Masonry>
        <el-empty v-else description="Chưa có dữ liệu"/>
      </template>
      <add-photo-dialog ref="addPhotoDialog" @onAddPhotoSucceed="onAddPhotoSucceed"></add-photo-dialog>
      <edit-photo-dialog ref="editPhotoDialog" @onUpdateSucceed="onEditSucceed"></edit-photo-dialog>
    </el-main>
    <el-footer height="48px">
      <el-pagination
        class="py-3 text-center"
        @size-change="handlePageSizeChange"
        @current-change="handleCurrentPageChange"
        :current-page="currentPage"
        :page-sizes="[10, 20, 40, 60, 80, 100]"
        :page-size="pageSize"
        background
        layout="total, sizes, prev, pager, next, jumper"
        :total="totalCount">
      </el-pagination>
    </el-footer>
  </el-container>
</template>

<script>
import { baseUrl } from "@/utils/global"
import addPhotoDialog from "@/views/Photography/AddPhotoDialog"
import editPhotoDialog from "@/views/Photography/EditPhotoDialog"
import Masonry from "@/components/Masonry/index.vue"
import PhotoCard from "@/components/PhotoCard";

export default {
  name: "Photos",
  components: {
    addPhotoDialog,
    editPhotoDialog,
    Masonry,
    PhotoCard
  },
  data() {
    return {
      currentPage: 1,
      pageSize: 20,
      totalCount: 0,
      allPhotos: [],
      photos: [],
      loading: false,
      // Chọn và hàng loạt
      selectionMode: false,
      selectedIds: [],
      // Tìm kiếm và lọc
      searchKeyword: '',
      featuredFilter: 'all', // all | featured | unfeatured
      filterTimer: null,
      guideVisible: true,
      featuredIdSet: new Set(),
    }
  },
  mounted() {
    this.loadPhotos()
  },
  methods: {
    async loadPhotos() {
      this.loading = true
      console.log('Bắt đầu tải ảnh...')
      try {
        const res = await this.$api.photo.getList(this.currentPage, this.pageSize)
        console.log('Phản hồi API:', res)
        this.totalCount = res.pagination.totalItemCount
        this.allPhotos = res.data.map(item => ({
          ...item,
          url: `${baseUrl}/media/photography/${item.id}.jpg`,
          isFeatured: false,
          // Thêm kích thước mặc định cho masonry, thực tế sẽ tự thích ứng theo ảnh
          width: 260,
          height: Math.floor(Math.random() * 200) + 200, // Chiều cao ngẫu nhiên 200-400px
        }))
        console.log('Dữ liệu ảnh sau khi xử lý:', this.allPhotos)
        // Chờ đồng bộ trạng thái đề xuất hoàn tất rồi mới áp dụng bộ lọc
        await this.syncFeatured()
        this.applyFilters()
        console.log('Ảnh hiển thị cuối cùng:', this.photos)
      } catch (err) {
        console.error('Tải ảnh thất bại:', err)
        this.$message.error(`Tải ảnh thất bại: ${err.message || err}`)
      } finally {
        this.loading = false
        console.log('Tải xong, trạng thái loading:', this.loading)
      }
    },
    async syncFeatured() {
      console.log('Bắt đầu đồng bộ trạng thái đề xuất...')
      try {
        const resFeatured = await this.$api.featuredPhoto.getAll()
        console.log('Phản hồi API ảnh đề xuất:', resFeatured)
        const ids = new Set((resFeatured.data || []).map(fp => fp.photo.id))
        this.featuredIdSet = ids
        this.allPhotos = this.allPhotos.map(p => ({ ...p, isFeatured: ids.has(p.id) }))
        console.log('Ảnh sau khi đồng bộ trạng thái đề xuất:', this.allPhotos)
      } catch (err) {
        console.error('Lấy trạng thái đề xuất thất bại:', err)
        // Lỗi không nghiêm trọng, chỉ thông báo và không làm gián đoạn
        this.$message.warning(`Lấy trạng thái đề xuất thất bại: ${err.message || err}`)
      }
    },
    applyFilters() {
      console.log('Áp dụng bộ lọc, điều kiện hiện tại:', { searchKeyword: this.searchKeyword, featuredFilter: this.featuredFilter })
      const kw = (this.searchKeyword || '').trim().toLowerCase()
      let list = this.allPhotos.slice()
      console.log('Số lượng ảnh trước khi lọc:', list.length)
      if (kw) {
        list = list.filter(p => (p.title || '').toLowerCase().includes(kw) || (p.location || '').toLowerCase().includes(kw))
        console.log('Số lượng ảnh sau khi tìm kiếm và lọc:', list.length)
      }
      if (this.featuredFilter === 'featured') {
        list = list.filter(p => p.isFeatured)
        console.log('Số lượng ảnh đề xuất sau khi lọc:', list.length)
      } else if (this.featuredFilter === 'unfeatured') {
        list = list.filter(p => !p.isFeatured)
        console.log('Số lượng ảnh chưa đề xuất sau khi lọc:', list.length)
      }
      this.photos = list
      console.log('Kết quả lọc cuối cùng:', this.photos)
    },
    onFilterInput() {
      clearTimeout(this.filterTimer)
      this.filterTimer = setTimeout(() => this.applyFilters(), 350)
    },
    clearFilters() {
      this.searchKeyword = ''
      this.featuredFilter = 'all'
      this.applyFilters()
    },
    // Chế độ chọn
    toggleSelectionMode() {
      this.selectionMode = !this.selectionMode
      if (!this.selectionMode) {
        this.selectedIds = []
      }
    },
    onCardSelectChange({ id, selected }) {
      const idx = this.selectedIds.indexOf(id)
      if (selected && idx < 0) this.selectedIds.push(id)
      if (!selected && idx >= 0) this.selectedIds.splice(idx, 1)
    },
    onCardFeaturedChange({ id, isFeatured }) {
      const target = this.allPhotos.find(p => p.id === id)
      if (target) {
        target.isFeatured = isFeatured
        this.applyFilters()
      }
    },
    // Thao tác hàng loạt
    async handleBatchDelete() {
      if (this.selectedIds.length === 0) return
      try {
        await this.$confirm(`Sẽ xóa ${this.selectedIds.length} ảnh đã chọn, bạn có muốn tiếp tục không?`, 'Xác nhận', { type: 'warning' })
        await Promise.all(this.selectedIds.map(id => this.$api.photo.deleteItem(id)))
        this.$message.success('Xóa hàng loạt thành công')
        this.selectedIds = []
        await this.loadPhotos()
      } catch (err) {
        if (err !== 'cancel') this.$message.error(`Xóa hàng loạt thất bại: ${err.message || err}`)
      }
    },
    async handleBatchSetFeatured() {
      if (this.selectedIds.length === 0) return
      try {
        await Promise.all(this.selectedIds.map(id => this.$api.photo.setFeatured(id)))
        this.$message.success('Đặt làm đề xuất hàng loạt thành công')
        await this.loadPhotos()
      } catch (err) {
        this.$message.error(`Thiết lập hàng loạt thất bại: ${err.message || err}`)
      }
    },
    async handleBatchCancelFeatured() {
      if (this.selectedIds.length === 0) return
      try {
        await Promise.all(this.selectedIds.map(id => this.$api.photo.cancelFeatured(id)))
        this.$message.success('Bỏ đề xuất hàng loạt thành công')
        await this.loadPhotos()
      } catch (err) {
        this.$message.error(`Hủy hàng loạt thất bại: ${err.message || err}`)
      }
    },
    // Phân trang
    handlePageSizeChange(pageSize) {
      this.pageSize = pageSize
      this.loadPhotos()
    },
    handleCurrentPageChange(page) {
      this.currentPage = page
      this.loadPhotos()
    },
    onAddPhotoSucceed() {
      this.$message.success('Thêm thành công')
      this.loadPhotos()
    },
    openEditDialog(photo) {
      this.$refs.editPhotoDialog.show(photo)
    },
    onEditSucceed() {
      this.$message.success('Cập nhật thành công')
      this.loadPhotos()
    }
  }
}
</script>

<style scoped>
 .header-left > * { margin-right: 8px; }
 .header-right > * { margin-left: 8px; }
 .mt-2 { margin-top: 8px; }
 .mr-2 { margin-right: 8px; }
 .skeleton-grid { display: flex; flex-wrap: wrap; justify-content: center; }
 .image { width: 100%; display: block; }
 
</style>
