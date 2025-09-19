#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
背景去除程式
去除圖片中的白色背景，製作透明背景的遊戲動畫
"""

import cv2
import numpy as np
import os
from pathlib import Path

def remove_white_background(image, threshold=30, blur_kernel=5):
    """
    去除白色背景，製作透明背景
    
    Args:
        image: 輸入圖片 (BGR格式)
        threshold: 白色檢測閾值，數值越小越嚴格
        blur_kernel: 模糊核大小，用於平滑邊緣
    
    Returns:
        處理後的圖片 (BGRA格式，包含透明通道)
    """
    
    # 轉換為HSV色彩空間，更容易檢測白色
    hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)
    
    # 定義白色的HSV範圍
    # 白色在HSV中：H可以是任意值，S接近0，V接近255
    lower_white = np.array([0, 0, 255 - threshold])
    upper_white = np.array([180, threshold, 255])
    
    # 創建白色遮罩
    white_mask = cv2.inRange(hsv, lower_white, upper_white)
    
    # 使用形態學操作來改善遮罩
    kernel = np.ones((3, 3), np.uint8)
    white_mask = cv2.morphologyEx(white_mask, cv2.MORPH_CLOSE, kernel)
    white_mask = cv2.morphologyEx(white_mask, cv2.MORPH_OPEN, kernel)
    
    # 模糊遮罩邊緣，讓透明效果更自然
    if blur_kernel > 0:
        white_mask = cv2.GaussianBlur(white_mask, (blur_kernel, blur_kernel), 0)
    
    # 反轉遮罩（白色區域變為透明）
    alpha_mask = 255 - white_mask
    
    # 轉換為BGRA格式
    bgra_image = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
    
    # 應用透明通道
    bgra_image[:, :, 3] = alpha_mask
    
    return bgra_image

def process_images_in_folder(input_folder, output_folder, threshold=30, blur_kernel=5):
    """
    處理資料夾中的所有圖片
    
    Args:
        input_folder: 輸入資料夾路徑
        output_folder: 輸出資料夾路徑
        threshold: 白色檢測閾值
        blur_kernel: 模糊核大小
    """
    
    # 創建輸出資料夾
    os.makedirs(output_folder, exist_ok=True)
    
    # 支援的圖片格式
    image_extensions = ['.png', '.jpg', '.jpeg', '.bmp', '.tiff']
    
    # 獲取所有圖片文件
    image_files = []
    for ext in image_extensions:
        image_files.extend(Path(input_folder).glob(f"*{ext}"))
        image_files.extend(Path(input_folder).glob(f"*{ext.upper()}"))
    
    if not image_files:
        print(f"在 {input_folder} 中找不到圖片文件")
        return False
    
    print(f"找到 {len(image_files)} 個圖片文件")
    print(f"開始處理...")
    
    processed_count = 0
    
    for image_file in sorted(image_files):
        try:
            # 讀取圖片
            image = cv2.imread(str(image_file))
            
            if image is None:
                print(f"無法讀取圖片: {image_file}")
                continue
            
            # 去除白色背景
            processed_image = remove_white_background(image, threshold, blur_kernel)
            
            # 生成輸出文件名
            output_filename = f"neonisland_game_{processed_count + 1:06d}.png"
            output_path = os.path.join(output_folder, output_filename)
            
            # 保存處理後的圖片
            cv2.imwrite(output_path, processed_image)
            
            processed_count += 1
            print(f"已處理: {image_file.name} -> {output_filename}")
            
        except Exception as e:
            print(f"處理 {image_file} 時發生錯誤: {e}")
            continue
    
    print(f"\n✅ 背景去除完成！")
    print(f"成功處理了 {processed_count} 張圖片")
    print(f"處理後的圖片已保存到: {output_folder}")
    
    return True

def main():
    """主程式"""
    print("=== 背景去除程式 ===")
    print("去除圖片中的白色背景，製作透明背景的遊戲動畫\n")
    
    # 設定資料夾路徑
    input_folder = "screenshots"  # 截圖程式輸出的資料夾
    output_folder = "neonisland_game_processed"  # 處理後的圖片資料夾
    
    # 檢查輸入資料夾
    if not os.path.exists(input_folder):
        print(f"錯誤：找不到輸入資料夾 {input_folder}")
        print("請先執行影片截圖程式")
        return
    
    # 設定參數
    threshold = 30  # 白色檢測閾值，可以調整 (0-100)
    blur_kernel = 5  # 邊緣模糊核大小，可以調整 (0表示不模糊)
    
    print(f"處理參數：")
    print(f"  白色檢測閾值: {threshold}")
    print(f"  邊緣模糊核: {blur_kernel}")
    print(f"  輸入資料夾: {input_folder}")
    print(f"  輸出資料夾: {output_folder}")
    print()
    
    # 執行背景去除
    success = process_images_in_folder(input_folder, output_folder, threshold, blur_kernel)
    
    if success:
        print(f"\n🎉 所有圖片處理完成！")
        print(f"透明背景的遊戲動畫圖片已保存到: {output_folder}")
        print("\n提示：")
        print("- 如果背景去除效果不理想，可以調整 threshold 參數")
        print("- 如果邊緣太銳利，可以增加 blur_kernel 參數")
        print("- 處理後的PNG圖片可以直接在Unity中使用")
    else:
        print("\n❌ 處理失敗")

if __name__ == "__main__":
    main()
