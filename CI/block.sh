LOCK_FILE="${HOME}/mybuild.lock"

# create "lock" file.
touch "${LOCK_FILE}"
echo -e "Build paused. To resume it, open a SSH session to run '${YELLOW}rm ~/build.lock${NC}' command."
# wait until $HOME/build.lock deleted by user.
while [ -f "${LOCK_FILE}" ]; do
    sleep 1
done
echo "Build lock has been deleted. Resuming build."