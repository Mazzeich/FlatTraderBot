--Run = true

Settings = {
  Name = 'HLine',
  Value = 0,
  line = {
    {
      Name = 'HLine',
      Type = TYPE_LINE,
      Width = 5
    }
  }
}


function isMin(idxl, idx, idxr)
  if((idx.low < idxl.low) and (idx.low < idxr.low)) then
    return true
  else
    return false
  end
end


function isMax(idxl, idx, idxr)
  if((idx.high > idxl.high) and (idx.high > idxr.high)) then
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

local function reversedipairsiter(t, i)
    i = i - 1
    if i ~= 0 then
        return i, t[i]
    end
end
function reversedipairs(t)
    return reversedipairsiter, t, #t + 1
end


--------------------------------------------------------------
function main()  
  -- DataSource для работы со свечами на графике
  ds, errorDesk = CreateDataSource("QJSIM", "SBER", INTERVAL_M10)
  if ds == nil then
    message('[Connection error]: ' .. errorDesk)
  end
  
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
  
  local tag = "sberprice"
  local currentCandle = ds:Size() -- Текущая свеча
  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size()) -- Максимальное количество свечей не может быть больше общего количества свечей в таблице
  local todaysDay = os.date("%d") -- Текущий день месяца

  local tLines = getLinesCount(tag)
  local candlesTotal = getNumCandles(tag)
    message ("candles: " .. candlesTotal)

  tableCandle, n, lgnd = getCandlesByIndex(tag, 0, 0, candlesTotal)
  local coveredCandles = 30 -- Сойдёт для десятиминуток
  local maxes   = 0;
  local minis   = 0;
  local offset  = 0.5;
  -- ДОЛЖНО БЫТЬ (MAXES[i].H - MAXES[i-1].H) < OFFSET
  -- ИНАЧЕ MAXES=0, БОКОВИК НЕ НАЙДЕН. ВЕРНУТЬСЯ
  local cacheHigh = 0
  local cacheLow  = 0
  message("n-2: \n" .. n - 2) -- 57
  message("n - coveredCandles: " .. n - coveredCandles) -- 29
  local i = n - 2
  local twoHighFound = false
  local twoLowFound  = false
  while (i > n - coveredCandles)do
    message("i = " .. i .. "\nmaxes = " .. maxes .. "\nminis = " .. minis)
    i = i - 1
    -- Решение в лоб
    -- Свеча экстремальна?
    if(isMax(tableCandle[i-1], tableCandle[i], tableCandle[i+1]) == true) then
      if(cacheHigh == 0) then 
        cacheHigh = tableCandle[i].high
        goto continue
      elseif tableCandle[i].high > (cacheHigh + offset) then
        message("Breakup\nFlat not found")
        break
      elseif(tableCandle[i].high < (cacheHigh - offset)) then
        goto continue
      else twoHighFound = true
      end
    elseif(isMin(tableCandle[i-1], tableCandle[i], tableCandle[i+1]) == true) then
      if(cacheLow == 0) then
        cacheLow = tableCandle[i].low
        goto continue
      elseif(tableCandle[i].low < (cacheLow - offset)) then
        message("Breakdown\nFlat not found")
        break
      elseif(tableCandle[i].low > (cacheLow + offset)) then
        goto continue
      else twoLowFound = true
      end
    end

    if((twoHighFound == true) and (twoLowFound == true))then 
      message ("The flat is found!")
      break
    end
    
    ::continue::
  end
  
  -- Пока не нашли первую свечу дня либо не проверили все свечи на графике
  while ((firstCandleIndex == nil) and (currentCandle > ds:Size() - maxCandles)) do
      -- Если день этой свечи не совпадает с сегодняшним днём, тогда...
      if tonumber(ds:T(currentCandle).day) ~= todaysDay then
        firstCandleIndex = currentCandle - 1 -- ... = индекс искомой свечи
        --message ("Found index: " .. tostring (firstCandleIndex))
      end
      currentCandle = currentCandle - 1
  end
    
  return 0
end

--------------------------------------------------------------
--Run = false