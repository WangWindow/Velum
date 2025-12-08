#!/bin/bash
# 构建并运行 Velum 后端 Docker 容器（Alpine 版）
# 用法：sudo ./backend-docker.sh [build|run|all]

IMAGE_NAME="velum-backend:alpine"
CONTAINER_NAME="velum-backend"
HOST_PORT=17597
CONTAINER_PORT=17597
HTTPS_HOST_PORT=16796
HTTPS_CONTAINER_PORT=16796
DATA_DIR="./temp/docker-data" # 数据目录
TEMP_CERT_DIR="./temp/docker-certs-temp" # 构建时的临时证书目录

# --- 证书配置 (自动对接 1Panel) ---
# 请将下面的路径修改为您在服务器上使用 'find' 命令找到的实际路径
# 根据您的反馈，路径位于 /opt/1panel/tmp/ssl/azure.modestwang.cn
CERT_SOURCE_PATH="/opt/1panel/tmp/ssl/azure.modestwang.cn"

# 证书文件名 (1Panel 通常使用这些名称，如果不同请修改)
CERT_PUBLIC_KEY="fullchain.pem"
CERT_PRIVATE_KEY="privkey.pem"
# --------------------------------

set -e

function build_image() {
    echo "[构建镜像]"
    sudo docker build -t $IMAGE_NAME -f backend/Dockerfile backend
}

function run_container() {
    echo "[运行容器]"

    # 准备数据目录
    if [ ! -d "$DATA_DIR" ]; then
        echo "创建数据目录: $DATA_DIR"
        sudo mkdir -p "$DATA_DIR"
    fi

    # 设置数据目录权限 (UID 1654 是 .NET Docker 镜像中 app 用户的 ID)
    echo "设置数据目录 $DATA_DIR 权限..."
    sudo chown -R 1654:1654 "$DATA_DIR"
    sudo chmod 777 "$DATA_DIR"

    # 检查并准备 appsettings.local.json
    LOCAL_SETTINGS="backend/api/appsettings.local.json"
    MOUNT_SETTINGS=""
    if [ -f "$LOCAL_SETTINGS" ]; then
        echo "✅ 发现本地配置文件: $LOCAL_SETTINGS"
        MOUNT_SETTINGS="-v $(pwd)/$LOCAL_SETTINGS:/app/appsettings.local.json"
    else
        echo "⚠️ 警告: 未找到 $LOCAL_SETTINGS"
        echo "   如果需要使用自定义 API Key，请确保该文件存在。"
    fi

    # 检查证书目录是否存在
    if ! sudo test -d "$CERT_SOURCE_PATH"; then
        echo "❌ 错误: 找不到证书目录: $CERT_SOURCE_PATH"
        echo "请在服务器上运行以下命令查找正确路径，并修改脚本中的 CERT_SOURCE_PATH:"
        echo "sudo find /opt/1panel -name \"fullchain.pem\" | grep \"azure.modestwang.cn\""
        exit 1
    fi

    echo "✅ 已找到证书目录: $CERT_SOURCE_PATH"

    # 复制证书到临时目录并设置权限
    # 解决 1Panel 证书目录权限问题 (System.UnauthorizedAccessException)
    if [ ! -d "$TEMP_CERT_DIR" ]; then
        sudo mkdir -p "$TEMP_CERT_DIR"
    fi

    echo "复制证书到临时目录..."
    sudo cp "$CERT_SOURCE_PATH/$CERT_PUBLIC_KEY" "$TEMP_CERT_DIR/"
    sudo cp "$CERT_SOURCE_PATH/$CERT_PRIVATE_KEY" "$TEMP_CERT_DIR/"

    echo "设置证书权限..."
    sudo chown -R 1654:1654 "$TEMP_CERT_DIR"
    sudo chmod 644 "$TEMP_CERT_DIR/$CERT_PUBLIC_KEY"
    sudo chmod 644 "$TEMP_CERT_DIR/$CERT_PRIVATE_KEY"

    # 清理旧容器
    sudo docker rm -f $CONTAINER_NAME 2>/dev/null || true

    echo "启动容器..."
    sudo docker run -d \
        --name $CONTAINER_NAME \
        -p $HOST_PORT:$CONTAINER_PORT \
        -p $HTTPS_HOST_PORT:$HTTPS_CONTAINER_PORT \
        -v "$(pwd)/$DATA_DIR:/data" \
        -v "$(pwd)/$TEMP_CERT_DIR:/https:ro" \
        $MOUNT_SETTINGS \
        -e "ConnectionStrings__DefaultConnection=Data Source=/data/velum.db" \
        -e "ASPNETCORE_URLS=http://+:$CONTAINER_PORT;https://+:$HTTPS_CONTAINER_PORT" \
        -e "ASPNETCORE_Kestrel__Certificates__Default__Path=/https/$CERT_PUBLIC_KEY" \
        -e "ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/$CERT_PRIVATE_KEY" \
        $IMAGE_NAME

    echo "[容器已启动]"
    echo "HTTP:  http://localhost:$HOST_PORT"
    echo "HTTPS: https://localhost:$HTTPS_HOST_PORT"
}

case "$1" in
    build)
        build_image
        ;;
    run)
        run_container
        ;;
    all|"")
        build_image
        run_container
        ;;
    *)
        echo "用法: sudo ./backend-docker.sh [build|run|all]"
        exit 1
        ;;
esac
