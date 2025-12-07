#!/bin/bash
# 构建并运行 Velum 后端 Docker 容器（Alpine 版）
# 用法：sudo ./backend-docker.sh [build|run|all]

IMAGE_NAME="velum-backend:alpine"
CONTAINER_NAME="velum-backend"
HOST_PORT=17597
CONTAINER_PORT=17597
DATA_DIR="./docker-data"

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
        mkdir -p "$DATA_DIR"
    fi

    # 设置权限，确保容器内的 app 用户 (UID 1654) 可以写入
    # Alpine 镜像中 app 用户 ID 通常为 1654
    echo "设置数据目录权限..."
    sudo chown -R 1654:1654 "$DATA_DIR"

    # 先尝试停止并删除同名容器（如果存在）
    sudo docker rm -f $CONTAINER_NAME 2>/dev/null || true

    sudo docker run -d \
        --name $CONTAINER_NAME \
        -p $HOST_PORT:$CONTAINER_PORT \
        -v "$(pwd)/$DATA_DIR:/data" \
        -e "ConnectionStrings__DefaultConnection=Data Source=/data/velum.db" \
        $IMAGE_NAME
    echo "[容器已启动] http://localhost:$HOST_PORT"
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
