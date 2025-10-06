#!/usr/bin/env bash
set -euo pipefail

BASE_URL=${BASE_URL:-http://localhost:5129}
CLEANUP=${CLEANUP:-false}

echo "Starting smoke test against $BASE_URL"

SUFFIX=$(date +%s%N)
EMAIL="smoketest+${SUFFIX}@example.com"
PASSWORD='Sm0keTest!'

echo "Registering user: $EMAIL"
REG=$(curl -s -X POST "$BASE_URL/api/auth/register" -H 'Content-Type: application/json' -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\",\"name\":\"Smoke Tester\"}")
echo "Register response: $REG"

TOKEN=$(echo "$REG" | jq -r '.token')
USERID=$(echo "$REG" | jq -r '.user.id')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "Registration failed or returned no token"
  echo "$REG"
  exit 1
fi

echo "Logging in to verify token"
LOGIN=$(curl -s -X POST "$BASE_URL/api/auth/login" -H 'Content-Type: application/json' -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
echo "Login response: $LOGIN"

TOKEN=$(echo "$LOGIN" | jq -r '.token')
if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "Login failed or returned no token"
  echo "$LOGIN"
  exit 2
fi

echo "Creating a task for user $USERID"
TASK=$(curl -s -X POST "$BASE_URL/api/tasks" -H 'Content-Type: application/json' -H "Authorization: Bearer $TOKEN" -d "{\"userId\":\"$USERID\",\"title\":\"Smoke Test Task\",\"description\":\"Created by smoke-test.sh\",\"status\":0,\"priority\":1,\"estimatedHours\":1.0,\"dueDate\":null,\"tags\":[\"smoke\"],\"dependencies\":[],\"energyLevel\":3,\"focusTimeMinutes\":30,\"goalId\":null}")
echo "Create task response: $TASK"

TASKID=$(echo "$TASK" | jq -r '.data.id')
if [ -z "$TASKID" ] || [ "$TASKID" = "null" ]; then
  echo "Create task failed"
  echo "$TASK"
  exit 3
fi

echo "Task created: $TASKID"

if [ "$CLEANUP" = "true" ]; then
  echo "Cleaning up: deleting task $TASKID"
  DEL=$(curl -s -X DELETE "$BASE_URL/api/tasks/$TASKID" -H "Authorization: Bearer $TOKEN")
  echo "Delete response: $DEL"
fi

echo "Smoke test completed successfully"
jq -n --arg user "$USERID" --arg email "$EMAIL" --arg task "$TASKID" '{success:true, user:{id:$user,email:$email}, task:{id:$task}}'
