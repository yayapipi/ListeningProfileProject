#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
影片處理批次腳本
自動執行影片截圖和背景去除兩個步驟
"""

import os
import sys
import subprocess

def run_script(script_name):
    """執行Python腳本"""
    try:
        print(f"\n{'='*50}")
        print(f"執行: {script_name}")
        print(f"{'='*50}")
        
        result = subprocess.run([sys.executable, script_name], 
                              capture_output=True, text=True, encoding='utf-8')
        
        print(result.stdout)
        if result.stderr:
            print("錯誤訊息:")
            print(result.stderr)
        
        return result.returncode == 0
        
    except Exception as e:
        print(f"執行 {script_name} 時發生錯誤: {e}")
        return False

def main():
    """主程式"""
    print("=== 影片處理批次腳本 ===")
    print("自動執行影片截圖和背景去除\n")
    
    # 檢查必要文件
    required_files = ["video_screenshot.py", "background_removal.py"]
    missing_files = [f for f in required_files if not os.path.exists(f)]
    
    if missing_files:
        print(f"錯誤：找不到必要文件: {', '.join(missing_files)}")
        return
    
    # 檢查影片文件
    video_file = r"D:\AI\怪奇遊戲\霓虹島ui\neonislandui1.mp4"
    if not os.path.exists(video_file):
        print(f"找不到影片文件: {video_file}")
        print("請檢查路徑是否正確")
        return
    
    print(f"找到影片文件: {video_file}")
    print("開始處理...\n")
    
    # 步驟1：影片截圖
    print("步驟 1/2: 影片截圖")
    success1 = run_script("video_screenshot.py")
    
    if not success1:
        print("❌ 影片截圖失敗，停止處理")
        return
    
    # 步驟2：背景去除
    print("\n步驟 2/2: 背景去除")
    success2 = run_script("background_removal.py")
    
    if not success2:
        print("❌ 背景去除失敗")
        return
    
    # 完成
    print(f"\n{'='*50}")
    print("🎉 所有處理完成！")
    print(f"{'='*50}")
    print("處理結果：")
    print("- 原始截圖保存在: screenshots/")
    print("- 透明背景圖片保存在: neonisland_game_processed/")
    print("- 可以直接在Unity中使用處理後的PNG圖片")

if __name__ == "__main__":
    main()
