--Run = true
function isMin(idxl, idx, idxr)
  if((idx.high <= idxl.high) and (idx.high <= idxr.high)) then
    return true
  else
    return false
  end
end


function isMax(idxl, idx, idxr)
  if((idx.high >= idxl.high) and (idx.high >= idxr.high)) then
    return true
  else
    return false
  end
end

-- Вызывается при установлении соединения с сервером  
function OnConnected()
  if (OnConnected()) then
    message('[Connected to the server]')
  else
    message('[Cannot connect to the server]')
  end
end
--------------------------------------------------------------
function main()  
  -- DataSource для работы со свечами на графике
  ds, errorDesk = CreateDataSource("QJSIM", "ROSN", INTERVAL_M1)
  local tag = "rosprice"
  if ds == nil then
    message('[Connection error]: ' .. errorDesk)
  end
  
  local minWidth = 0.02

  -- Проверка, загрузился ли график
  while (errorDesk == "" or errorDesk == nil) and ds:Size() == 0 do
    sleep(1)
  end
  if ((errorDesk ~= "") and (errorDesk ~= nil)) then 
    message ('[Unable to connect to the chart...]: ' .. errorDesk)
    return 0
  end
  
  -- Ограничиваем количество попыток ожидания получения данных от сервера
  local try_count = 0
  while ((ds:Size() == 0) and (try_count < 1000)) do
    sleep(100)
    try_count = try_count + 1
  end
  
  local currentCandle = ds:Size() -- Текущая свеча
  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size()) -- Максимальное количество свечей не может быть больше общего количества свечей в таблице
  local nowsMinute = os.date("%M") -- Текущий день месяца

  local tLines = getLinesCount(tag)
  local candlesTotal = getNumCandles(tag)
  message ("candles: " .. candlesTotal)

  tableCandle, n, lgnd = getCandlesByIndex(tag, 0, 0, candlesTotal)
  local coveredCandles = 70
  local maxes   = 0;
  local minis   = 0;
  local offset  = 0.05;
  local cacheHigh = 0
  local cacheLow  = 0
  
  message("n - 1: \n" .. n - 1) 
  message("n - coveredCandles: " .. n - coveredCandles) 

  local i = n - 10
  local twoHighFound = false
  local twoLowFound  = false
  while (i > n - coveredCandles) do

    local dateCandle = tableCandle[i].datetime

    -- Решение в лоб
    --message("twoHighFound = " .. tostring(twoHighFound) .. "\ntwoLowFound = " .. tostring(twoLowFound))
    i = i - 1
    -- Свеча -- локальный максимум?
    if(isMax(tableCandle[i-1], tableCandle[i], tableCandle[i+1]) == true) then
      if(cacheHigh == 0) then 
        cacheHigh = tableCandle[i].high
        goto continue
      elseif tableCandle[i].high > (cacheHigh + offset) then
        message("Breakup on " .. tostring(dateCandle.hour) .. ":" .. tostring(dateCandle.min))
        break
      elseif(tableCandle[i].high < (cacheHigh - offset)) then
        goto continue
      else twoHighFound = true
      end
    -- Свеча -- локальный минимум?
    elseif(isMin(tableCandle[i-1], tableCandle[i], tableCandle[i+1]) == true) then
      if(cacheLow == 0) then
        cacheLow = tableCandle[i].high
        goto continue
      elseif(tableCandle[i].high < (cacheLow - offset)) then
        message("Breakdown on " .. tostring(dateCandle.hour) .. ":" .. tostring(dateCandle.min))
        break
      elseif(tableCandle[i].high > (cacheLow + offset)) then
        goto continue
      else twoLowFound = true
      end
    end

    -- Теперь обработаем случай, когда первый найденный экстремум находится между двумя следующими
    if((cacheHigh ~= 0) and (math.abs(cacheLow - tableCandle[i].high) > (minWidth * tableCandle[i].close))) then
      cacheHigh = 0
    elseif((cacheLow ~= 0) and (math.abs(cacheHigh - tableCandle[i].high) > (minWidth * tableCandle[i].close))) then
      cacheLow = 0
    end

    if((twoHighFound == true) and (twoLowFound == true))then
      --message("twoHighFound = " .. tostring(twoHighFound) .. "\ntwoLowFound = " .. tostring(twoLowFound))
      message("The flat is found on " .. tostring(dateCandle.hour) .. ":" .. tostring(dateCandle.min))
      break
    end
    
    ::continue::
  end
  
  -- Пока не нашли первую свечу дня либо не проверили все свечи на графике
  while ((firstCandleIndex == nil) and (currentCandle > ds:Size() - maxCandles)) do
      -- Если день этой свечи не совпадает с сегодняшним днём, тогда...
      if tonumber(ds:T(currentCandle).min) ~= nowsMinute then
        firstCandleIndex = currentCandle - 1 -- ... = индекс искомой свечи
        --message ("Found index: " .. tostring (firstCandleIndex))
      end
      currentCandle = currentCandle - 1
  end
    
  return 0
end

--------------------------------------------------------------
--Run = false