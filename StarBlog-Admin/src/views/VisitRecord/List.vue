<template>
  <el-container>
    <el-header height="30px">
      <el-row type="flex" justify="space-between">
        <el-row :gutter="10">
          <el-col :span="3">
            <el-input v-model="search"
                      placeholder="Vui lòng nhập từ khóa" prefix-icon="el-icon-search"></el-input>
          </el-col>
          <el-col :span="2">
            <el-select v-model="excludeApi" clearable placeholder="Lọc API">
              <el-option label="Có" :value="true"/>
              <el-option label="Không" :value="false"/>
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="excludeIntranetIp" clearable placeholder="Lọc mạng nội bộ">
              <el-option label="Có" :value="true"/>
              <el-option label="Không" :value="false"/>
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="geoFilter.country" clearable filterable placeholder="Quốc gia"
                       @change="handleCountryChange">
              <el-option
                v-for="item in geoFilterParams.country"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="geoFilter.province" clearable filterable placeholder="Tỉnh/thành"
                       @change="handleProvinceChange">
              <el-option
                v-for="item in geoFilterParams.province"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="geoFilter.city" clearable filterable placeholder="Thành phố">
              <el-option
                v-for="item in geoFilterParams.city"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>

          <el-col :span="2">
            <el-select v-model="userAgentFilter.os" clearable filterable placeholder="Hệ điều hành">
              <el-option
                v-for="item in userAgentFilterParams.os"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="userAgentFilter.device" clearable filterable placeholder="Thiết bị">
              <el-option
                v-for="item in userAgentFilterParams.device"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="userAgentFilter.userAgent" clearable filterable placeholder="UserAgent">
              <el-option
                v-for="item in userAgentFilterParams.userAgent"
                :key="item" :label="item" :value="item"
              />
            </el-select>
          </el-col>
          <el-col :span="2">
            <el-select v-model="userAgentFilter.isSpider" clearable placeholder="Có phải crawler hay không">
              <el-option label="Có" :value="true"/>
              <el-option label="Không" :value="false"/>
            </el-select>
          </el-col>

          <el-col :span="2">
            <el-button @click="loadVisitRecord">Tìm kiếm</el-button>
          </el-col>
        </el-row>
        <div>
          <el-button @click="">Bỏ chọn</el-button>
        </div>
      </el-row>
    </el-header>
    <el-main>
      <el-table
        ref="table"
        v-loading="loading"
        :data="data"
        stripe
        style="width: 100%"
        :default-sort="{prop: 'time',order:'descending'}"
        @sort-change="handleSortChange">
        <el-table-column
          type="selection"
          width="30"/>
        <el-table-column
          prop="timeStr"
          label="Thời gian"
          sortable
          width="200"/>
        <el-table-column
          prop="requestPath"
          label="Địa chỉ yêu cầu"
          sortable
          :show-overflow-tooltip="true"
        >
          <template v-slot="scope">
            <el-link type="text" :href="`${baseUrl}${scope.row.requestPath}`" target="_blank">
              {{ scope.row.requestPath }}
            </el-link>
          </template>
        </el-table-column>
        <el-table-column
          prop="requestMethod"
          label="Phương thức HTTP"
          sortable
          width="150"/>
        <el-table-column
          prop="statusCode"
          label="Mã trạng thái"/>
        <el-table-column
          prop="responseTimeMs"
          sortable
          label="Thời gian phản hồi"/>
        <el-table-column
          prop="ipInfo.country"
          sortable
          label="Quốc gia"/>
        <el-table-column
          prop="ipInfo.province"
          sortable
          label="Tỉnh"/>
        <el-table-column
          prop="ipInfo.city"
          sortable
          label="Thành phố"/>
        <el-table-column
          prop="ipInfo.isp"
          sortable
          label="Nhà mạng"/>
        <el-table-column
          prop="userAgentInfo.os.family"
          sortable
          label="Hệ điều hành"/>
        <el-table-column
          prop="userAgentInfo.device.family"
          sortable
          label="Thiết bị"/>
        <el-table-column
          prop="userAgentInfo.device.isSpider"
          sortable
          label="Crawler">
          <template v-slot="scope">
            <el-tag v-if="scope.row.userAgentInfo.device.isSpider">Crawler</el-tag>
            <el-tag type="info" v-else>Người dùng</el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="userAgentInfo.userAgent.family"
          label="UserAgent"/>
        <el-table-column
          align="right"
          label="Thao tác">
          <template v-slot="scope">
            <el-link type="text" @click="showDetail(scope.row)">Chi tiết</el-link>
          </template>
        </el-table-column>
      </el-table>

      <!-- Phân trang -->
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
    </el-main>
    <el-dialog :visible.sync="dialogVisible" title="Chi tiết lịch sử truy cập" width="50%">
      <div v-if="selectedRecord">
        <el-row :gutter="20">
          <el-col :span="12">
            <p><strong>ID:</strong> {{ selectedRecord.id }}</p>
            <p><strong>IP:</strong> {{ selectedRecord.ip }}</p>
            <p><strong>Mã khu vực:</strong> {{ selectedRecord.ipInfo.regionCode }}</p>
            <p><strong>Quốc gia:</strong> {{ selectedRecord.ipInfo.country }}</p>
            <p><strong>Tỉnh/thành:</strong> {{ selectedRecord.ipInfo.province }}</p>
            <p><strong>Thành phố:</strong> {{ selectedRecord.ipInfo.city }}</p>
            <p><strong>Nhà mạng:</strong> {{ selectedRecord.ipInfo.isp }}</p>
            <p><strong>Địa chỉ yêu cầu:</strong>
              <el-link type="text" :href="`${baseUrl}${selectedRecord.requestPath}`" target="_blank">
                {{ selectedRecord.requestPath }}
              </el-link>
            </p>
            <p><strong>Tham số yêu cầu:</strong> {{ selectedRecord.requestQueryString || 'Không' }}</p>
            <p><strong>Phương thức yêu cầu:</strong> {{ selectedRecord.requestMethod }}</p>
            <p><strong>Mã trạng thái:</strong> {{ selectedRecord.statusCode }}</p>
            <p><strong>Thời gian phản hồi:</strong> {{ selectedRecord.responseTimeMs }} ms</p>
          </el-col>
          <el-col :span="12">
            <p><strong>User Agent:</strong> {{ selectedRecord.userAgent }}</p>
            <p><strong>Hệ điều hành:</strong> {{ selectedRecord.userAgentInfo.os.family }}
              {{ selectedRecord.userAgentInfo.os.major }}.{{
                selectedRecord.userAgentInfo.os.minor
              }}.{{ selectedRecord.userAgentInfo.os.patch }}</p>
            <p><strong>Hãng thiết bị:</strong> {{ selectedRecord.userAgentInfo.device.brand }}</p>
            <p><strong>Mẫu thiết bị:</strong> {{ selectedRecord.userAgentInfo.device.model }}</p>
            <p><strong>Loại thiết bị:</strong> {{ selectedRecord.userAgentInfo.device.family }}</p>
            <p><strong>Trình duyệt:</strong> {{ selectedRecord.userAgentInfo.userAgent.family }}
              {{ selectedRecord.userAgentInfo.userAgent.major }}.{{
                selectedRecord.userAgentInfo.userAgent.minor
              }}.{{ selectedRecord.userAgentInfo.userAgent.patch }}</p>
            <p><strong>Crawler:</strong> {{ selectedRecord.userAgentInfo.device.isSpider ? 'Có' : 'Không' }}</p>
            <p><strong>Thời gian truy cập:</strong> {{ selectedRecord.timeStr }}</p>
            <p><strong>Trang nguồn:</strong> {{ selectedRecord.referrer || 'Không' }}</p>
          </el-col>
        </el-row>
      </div>
      <span slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">Đóng</el-button>
      </span>
    </el-dialog>
  </el-container>
</template>

<script>
import * as utils from '@/utils/dateTime'
import {baseUrl} from "@/utils/global";


export default {
  name: "VisitRecordList",
  data() {
    return {
      baseUrl: baseUrl,
      loading: false,
      currentPage: 1,
      pageSize: 20,
      totalCount: 1000,
      search: '',
      sortBy: null,
      excludeApi: null,
      excludeIntranetIp: null,
      geoFilter: {
        param: 'country',
        country: null,
        province: null,
        city: null,
      },
      geoFilterParams: {
        country: [],
        province: [],
        city: [],
      },
      userAgentFilter: {
        os: null,
        device: null,
        userAgent: null,
        isSpider: null,
      },
      userAgentFilterParams: {
        os: [],
        device: [],
        userAgent: [],
      },
      data: [],
      dialogVisible: false,
      selectedRecord: null,
    }
  },
  mounted() {
    this.loadGeoFilterParams()
    this.loadUserAgentFilterParams()
    this.loadVisitRecord()
  },
  methods: {
    loadGeoFilterParams: function () {
      this.$api.visitRecord.getGeoFilterParams({...this.geoFilter})
        .then(res => {
          this.geoFilterParams[this.geoFilter.param] = res.data
        })
        .catch(res => this.$message.error(`Lỗi khi lấy tham số lọc thông tin địa lý: ${res.message}`))
    },
    loadUserAgentFilterParams: function () {
      this.$api.visitRecord.getUserAgentFilterParams()
        .then(res => {
          this.userAgentFilterParams = res.data
        })
        .catch(res => this.$message.error(`Lỗi khi lấy tham số lọc UserAgent: ${res.message}`))
    },
    loadVisitRecord: function () {
      this.loading = true
      this.$api.visitRecord.getList({
        excludeApi: this.excludeApi,
        excludeIntranetIp: this.excludeIntranetIp,
        country: this.geoFilter.country,
        province: this.geoFilter.province,
        os: this.userAgentFilter.os,
        device: this.userAgentFilter.device,
        userAgent: this.userAgentFilter.userAgent,
        isSpider: this.userAgentFilter.isSpider,
        city: this.geoFilter.city,
        page: this.currentPage,
        pageSize: this.pageSize,
        sortBy: this.sortBy
      })
        .then(res => {
          console.log(res)
          this.totalCount = res.pagination.totalItemCount
          this.data = res.data
          this.data.forEach(item => {
            item.timeStr = utils.dateTimeBeautify(item.time)
          })
        })
        .catch(res => this.$message.error(`Lỗi khi lấy danh sách lịch sử truy cập: ${res.message}`))
        .finally(() => this.loading = false)
    },
    showDetail(item) {
      this.selectedRecord = item;

      this.dialogVisible = true;
    },
    handleCountryChange: function (val) {
      this.geoFilter.province = null
      this.geoFilter.city = null
      this.geoFilter.param = 'province'
      this.loadGeoFilterParams()
    },
    handleProvinceChange: function (val) {
      this.geoFilter.city = null
      this.geoFilter.param = 'city'
      this.loadGeoFilterParams()
    },
    handleSortChange: function (column) {
      let orderProp = column.prop
      orderProp = orderProp.replaceAll('timeStr', 'time')

      if (column.order === 'ascending') {
        this.sortBy = orderProp
      } else if (column.order === 'descending') {
        this.sortBy = '-' + orderProp
      } else {
        this.sortBy = null
      }
      this.loadVisitRecord()
    },
    handlePageSizeChange(pageSize) {
      this.pageSize = pageSize
      this.loadVisitRecord()
    },
    handleCurrentPageChange(page) {
      this.currentPage = page
      this.loadVisitRecord()
    },
  }
}
</script>

<style scoped>

</style>
