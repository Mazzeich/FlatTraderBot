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


function isMin(table, idx)
  if((table[idx].low < table[idx-1].low) and (table[idx].low < table[idx+1].low)) then
    return true
  else
    return false
  end
end


function isMax(table)
  if((table[idx].high < table[idx-1].high) and (table[idx].high < table[idx+1].high)) then
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
  local coveredCandles = 30
  local maxes = 0;
  local minis = 0;
  local offset = 0.5;
  -- (MAXES[i].H - MAXES[i-1]) < OFFSET
  -- ИНАЧЕ MAXES=0, БОКОВИК НЕ НАЙДЕН. ВЕРНУТЬСЯ
  for i = n - 2, n - coveredCandles do
    if(isMax(tableCandle, i) == true) then
      maxes = maxes + 1;
      if(maxes > 2) then
        message("The flat not founded!") 
        break
      else goto continue
      end
    end

    if(isMin(tableCandle, i) == true) then
      minis = minis + 1;
      if(minis > 2) then
        message("The flat not founded!") 
        break
      else goto continue
      end
    end

    if((maxes == 2) and (minis == 2))then 
      message ("The flat found!")
      break
    end
    
    ::continue::
  end
  
  -- Пока не нашли первую свечу дня либо не проверили все свечи на графике
  while ((firstCandleIndex == nil) and (currentCandle > ds:Size() - maxCandles)) do
      -- Если день этой свечи не совпадает с сегодняшним днём, тогда...
      if tonumber(ds:T(currentCandle).day) ~= todaysDay then
        firstCandleIndex = currentCandle - 1 -- ... = индекс искомой свечи
        message ("Found index: " .. tostring (firstCandleIndex))
      end
      currentCandle = currentCandle - 1
  end
    
  return 0
end

--------------------------------------------------------------
--Run = false