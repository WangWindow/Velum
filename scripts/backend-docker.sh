#!/bin/bash
# 构建并运行 Velum 后端 Docker 容器（Alpine 版）
# 用法：sudo ./backend-docker.sh [build|run|all]

IMAGE_NAME="velum-backend:alpine"
CONTAINER_NAME="velum-backend"
HOST_PORT=17597
CONTAINER_PORT=17597

set -e

function build_image() {
    echo "[构建镜像]"
    sudo docker build -t $IMAGE_NAME -f backend/Dockerfile backend
}

function run_container() {
    echo "[运行容器]"
    # 先尝试停止并删除同名容器（如果存在）
    sudo docker rm -f $CONTAINER_NAME 2>/dev/null || true
    sudo docker run -d \
        --name $CONTAINER_NAME \
        -p $HOST_PORT:$CONTAINER_PORT \
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
