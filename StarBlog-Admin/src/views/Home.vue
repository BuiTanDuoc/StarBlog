<template>
  <div class="text-center">
    <el-row :gutter="8">
      <el-col :span="4">
        <!--Tổng lượt đọc-->
        <el-card v-loading="loading">
          <div slot="header">Tổng lượt truy cập</div>
          <h1>{{ visitRecordOverview.total }}</h1>
          <div>
            Pv / Uv
            <i class="el-icon-top text-primary"></i>
            {{ visitRecordOverview.pv }} / {{ visitRecordOverview.uv }}
          </div>
        </el-card>
        <!--Sáng tác mới-->
        <el-card class="mt-2">
          <div slot="header">Sáng tác mới</div>
          <el-row :gutter="8">
            <el-col :span="12">
              <el-button type="warning" plain class="w-100" @click="$router.push('/post/new')">
                <div><i class="icon-lg el-icon-tickets"></i></div>
                <div class="mt-2">Bài viết blog</div>
              </el-button>
            </el-col>
            <el-col :span="12">
              <el-button type="primary" plain class="w-100" @click="$router.push('/category/list')">
                <div><i class="icon-lg el-icon-folder"></i></div>
                <div class="mt-2">Danh mục bài viết</div>
              </el-button>
            </el-col>
          </el-row>
          <el-row :gutter="8" class="mt-2">
            <el-col :span="12">
              <el-button type="primary" plain class="w-100" @click="$router.push('/photo/list')">
                <div><i class="icon-lg el-icon-picture-outline"></i></div>
                <div class="mt-2">Tác phẩm nhiếp ảnh</div>
              </el-button>
            </el-col>
            <el-col :span="12">
              <el-button type="primary" plain class="w-100" @click="notImpl">
                <div><i class="icon-lg fa fa-code"></i></div>
                <div class="mt-2">Đoạn mã</div>
              </el-button>
            </el-col>
          </el-row>
        </el-card>
        <!--Thao tác nhanh-->
        <el-card class="mt-2">
          <div slot="header">Thao tác nhanh</div>
          <div>
            <el-button type="info" plain class="w-100" @click="notImpl">Nhập bài viết hàng loạt</el-button>
          </div>
          <div class="mt-2">
            <el-button type="info" plain class="w-100" @click="$router.push('post/upload')">Tải bài viết lên</el-button>
          </div>
          <div class="mt-2">
            <el-button type="info" plain class="w-100" @click="notImpl">Nhập ảnh hàng loạt</el-button>
          </div>
          <div class="mt-2">
            <el-button type="info" plain class="w-100" @click="$router.push('/photo/list')">Tải ảnh lên</el-button>
          </div>
          <div class="mt-2">
            <el-button type="info" plain class="w-100" @click="notImpl">Xuất dữ liệu</el-button>
          </div>
        </el-card>
      </el-col>
      <el-col :span="20">
        <el-row :gutter="8" v-loading="loading">
          <el-col :span="4">
            <el-card>
              <div slot="header">Số lượng bài viết</div>
              <h1>{{ overview['postsCount'] }}</h1>
            </el-card>
          </el-col>
          <el-col :span="4">
            <el-card>
              <div slot="header">Số lượng danh mục</div>
              <h1>{{ overview['categoriesCount'] }}</h1>
            </el-card>
          </el-col>
          <el-col :span="4">
            <el-card>
              <div slot="header">Số lượng ảnh</div>
              <h1>{{ overview['photosCount'] }}</h1>
            </el-card>
          </el-col>
          <el-col :span="4">
            <el-card>
              <div slot="header">Bài viết đề xuất</div>
              <h1>{{ overview['featuredPostsCount'] }}</h1>
            </el-card>
          </el-col>
          <el-col :span="4">
            <el-card>
              <div slot="header">Danh mục đề xuất</div>
              <h1>{{ overview['featuredCategoriesCount'] }}</h1>
            </el-card>
          </el-col>
          <el-col :span="4">
            <el-card>
              <div slot="header">Ảnh đề xuất</div>
              <h1>{{ overview['featuredPhotosCount'] }}</h1>
            </el-card>
          </el-col>
        </el-row>
        <el-card class="mt-2" v-loading="loading">
          <div slot="header" class="d-flex align-items-center justify-content-between">
            <span>Xu hướng dữ liệu</span>
            <div style="width: 200px">
              <el-slider v-model="trendDays" :min="1" :max="14" :step="1" @change="handleTrendDaysChange">
                <template #default="{ value }">
                  <span>{{ value }} ngày</span>
                </template>
              </el-slider>
            </div>
          </div>
          <dv-charts class="mt-2" :style="'height: 550px'" :option="trendChartOption"/>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script>
export default {
  components: {},
  data() {
    return {
      overview: {},
      visitRecordOverview: {},
      trend: null,
      loadStage: 0,
      trendDays: 10
    }
  },
  computed: {
    trendChartOption() {
      let data = {}
      if (this.trend !== null) {
        data = {
          xAxis: {
            name: 'Ngày tháng',
            data: this.trend.map(item => item.date),
            nameTextStyle: {
              fill: '#333',
              fontSize: 20
            },
            axisLabel: {
              style: {
                fill: '#333',
                fontSize: 16,
                rotate: 0
              }
            }
          },
          yAxis: {
            name: 'Lượt xem',
            data: 'value',
            min: 0,
            nameTextStyle: {
              fill: '#333',
              fontSize: 20
            },
            axisLabel: {
              style: {
                fill: '#333',
                fontSize: 16,
                rotate: 0
              }
            }
          },
          legend: {
            data: ['Tổng lượt xem', 'PV', 'UV', 'API', 'Crawler'],
            bottom: 10,
          },
          series: [
            {
              name: 'Tổng lượt xem',
              data: this.trend.map(item => item.total),
              type: 'bar',
              gradient: {
                color: ['#37a2da', '#67e0e3']
              },
              label: {
                show: true,
                formatter: '{value} lần'
              },
            },
            {
              name: 'PV',
              data: this.trend.map(item => item.pv),
              type: 'bar',
              label: {
                show: true,
              },
            },
            {
              name: 'UV',
              data: this.trend.map(item => item.uv),
              type: 'bar',
              label: {
                show: true,
              },
            },
            {
              name: 'API',
              data: this.trend.map(item => item.api),
              type: 'bar',
              lineStyle: {
                lineDash: [5, 5]
              }
            },
            {
              name: 'Crawler',
              data: this.trend.map(item => item.spider),
              type: 'bar',
              label: {
                show: true,
              },
            },
          ]
        }
      }
      return data
    },
    loading() {
      return this.loadStage < 3
    }
  },
  created() {
    this.load()
  },
  methods: {
    load() {
      this.$api.blog.overview()
        .then(res => {
          this.overview = null
          this.overview = res.data
          this.loadStage++
        })
        .catch(res => this.$message.error(`Lấy thất bại! ${res.message}`))

      this.$api.visitRecord.getOverview()
        .then(res => {
          this.visitRecordOverview = null
          this.visitRecordOverview = res.data
          this.loadStage++
        })
        .catch(res => this.$message.error(`Lấy dữ liệu thống kê truy cập thất bại! ${res.message}`))

      this.$api.visitRecord.getTrend(this.trendDays)
        .then(res => {
          this.trend = null
          this.trend = res.data
          this.loadStage++
        })
        .catch(res => this.$message.error(`Lấy dữ liệu xu hướng truy cập thất bại! ${res.message}`))
    },
    notImpl() {
      this.$message.warning('Chức năng đang được phát triển…')
    },
    handleTrendDaysChange(value) {
      this.$api.visitRecord.getTrend(value)
        .then(res => {
          this.trend = null
          this.trend = res.data
        })
        .catch(res => this.$message.error(`Lấy dữ liệu xu hướng truy cập thất bại! ${res.message}`))
    }
  }
}
</script>

<style>
.icon-lg {
  font-size: 40px !important;
}

.el-card__header {
  padding: 8px 20px;
}
</style>
