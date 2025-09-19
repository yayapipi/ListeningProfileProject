#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
影片截圖程式
每秒截圖6張，用於製作遊戲動畫
"""

import cv2
import os
import sys
from pathlib import Path

def extract_frames(video_path, output_folder, frames_per_second=6):
    """
    從影片中提取幀，每秒提取指定數量的幀
    
    Args:
        video_path (str): 影片文件路徑
        output_folder (str): 輸出資料夾路徑
        frames_per_second (int): 每秒提取的幀數，預設為6
    """
    
    # 檢查影片文件是否存在
    if not os.path.exists(video_path):
        print(f"錯誤：找不到影片文件 {video_path}")
        return False
    
    # 創建輸出資料夾
    os.makedirs(output_folder, exist_ok=True)
    
    # 打開影片
    cap = cv2.VideoCapture(video_path)
    
    if not cap.isOpened():
        print(f"錯誤：無法打開影片文件 {video_path}")
        return False
    
    # 獲取影片資訊
    fps = cap.get(cv2.CAP_PROP_FPS)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    duration = total_frames / fps
    
    print(f"影片資訊：")
    print(f"  原始FPS: {fps:.2f}")
    print(f"  總幀數: {total_frames}")
    print(f"  影片長度: {duration:.2f} 秒")
    print(f"  每秒截圖: {frames_per_second} 張")
    
    # 計算截圖間隔
    frame_interval = int(fps / frames_per_second)
    print(f"  截圖間隔: 每 {frame_interval} 幀截圖一次")
    
    frame_count = 0
    saved_count = 0
    
    while True:
        ret, frame = cap.read()
        
        if not ret:
            break
        
        # 每隔指定幀數截圖一次
        if frame_count % frame_interval == 0:
            # 生成文件名：neonisland_game_000001.png
            filename = f"neonisland_game_{saved_count + 1:06d}.png"
            filepath = os.path.join(output_folder, filename)
            
            # 保存圖片
            cv2.imwrite(filepath, frame)
            saved_count += 1
            
            print(f"已保存: {filename} (第 {frame_count} 幀)")
        
        frame_count += 1
    
    cap.release()
    
    print(f"\n截圖完成！")
    print(f"總共保存了 {saved_count} 張圖片到 {output_folder}")
    
    return True

def main():
    """主程式"""
    print("=== 影片截圖程式 ===")
    print("每秒截圖6張，用於製作遊戲動畫\n")
    
    # 設定文件路徑
    video_file = r"D:\AI\怪奇遊戲\霓虹島ui\neonislandui1.mp4"
    output_folder = "screenshots"
    
    # 檢查影片文件
    if not os.path.exists(video_file):
        print(f"請將 {video_file} 放在與此程式相同的資料夾中")
        print("或者修改程式中的 video_file 變數為正確的影片路徑")
        return
    
    # 執行截圖
    success = extract_frames(video_file, output_folder, frames_per_second=6)
    
    if success:
        print(f"\n✅ 截圖完成！圖片已保存到 {output_folder} 資料夾")
        print("現在可以執行背景去除程式來處理這些圖片")
    else:
        print("\n❌ 截圖失敗")

if __name__ == "__main__":
    main()
