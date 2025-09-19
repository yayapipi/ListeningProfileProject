# 影片動畫處理工具

這個工具可以將影片轉換為遊戲動畫，包含影片截圖和背景去除兩個功能。

## 功能

1. **影片截圖** (`video_screenshot.py`): 每秒截圖6張
2. **背景去除** (`background_removal.py`): 去除白色背景，製作透明背景
3. **批次處理** (`process_video.py`): 一次性執行所有步驟

## 安裝需求

```bash
pip install -r requirements.txt
```

或者手動安裝：
```bash
pip install opencv-python numpy
```

## 使用方法

### 方法1：批次處理（推薦）

1. 將 `neonislandui1.mp4` 放在與程式相同的資料夾中
2. 執行批次處理腳本：
   ```bash
   python process_video.py
   ```

### 方法2：分步執行

1. **影片截圖**：
   ```bash
   python video_screenshot.py
   ```

2. **背景去除**：
   ```bash
   python background_removal.py
   ```

## 輸出結果

- `screenshots/`: 原始截圖
- `neonisland_game_processed/`: 透明背景的遊戲動畫圖片

## 參數調整

### 截圖參數
在 `video_screenshot.py` 中可以調整：
- `frames_per_second`: 每秒截圖數量（預設6張）

### 背景去除參數
在 `background_removal.py` 中可以調整：
- `threshold`: 白色檢測閾值（0-100，預設30）
- `blur_kernel`: 邊緣模糊核大小（預設5）

## 注意事項

1. 確保影片文件名為 `neonislandui1.mp4`
2. 背景去除主要針對白色背景，其他顏色背景可能需要調整參數
3. 處理後的PNG圖片可以直接在Unity中使用
4. 建議先用小段影片測試效果

## 故障排除

- **找不到影片文件**: 確保 `neonislandui1.mp4` 在正確位置
- **背景去除效果不佳**: 調整 `threshold` 參數
- **邊緣太銳利**: 增加 `blur_kernel` 參數
- **安裝套件失敗**: 使用 `pip install --upgrade pip` 更新pip
