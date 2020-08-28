function main()
  local openPath 	= "/dataOpen1.txt"
  local highPath 	= "C:/Projects/Lua/datahigh.txt"
  local lowPath 	= "C:/Projects/Lua/dataLow.txt"
  local closePath 	= "C:/Projects/Lua/dataClose.txt"
  local volumePath 	= "C:/Projects/Lua/dataVolume.txt"

  local openIO 	 = io.open(openPath, "w")
  local highIO 	 = io.open(highPath, "w")
  local lowIO 	 = io.open(lowPath, "w")
  local closeIO  = io.open(closePath, "w")
  local volumeIO = io.open(volumePath, "w")

  ds, errorDesk = CreateDataSource("QJSIM", "ROSN", INTERVAL_M1)
  local tag = "rosprice"
  if ds == nil then
    message('[Connection error]: ' .. errorDesk)
  end

  while (errorDesk == "" or errorDesk == nil) and ds:Size() == 0 do
    sleep(1)
  end
  if ((errorDesk ~= "") and (errorDesk ~= nil)) then 
    message ('[Unable to connect to the chart...]: ' .. errorDesk)
    return 0
  end

  local try_count = 0
  while ((ds:Size() == 0) and (try_count < 1000)) do
    sleep(100)
    try_count = try_count + 1
  end

  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size())
  local tLines = getLinesCount(tag)
  local candlesTotal = getNumCandles(tag)
  local coveredCandles = 70

  tableCandle, n, lgnd = getCandlesByIndex(tag, 0, 0, candlesTotal)

  local data = {}

--    local O = t[i].open; -- Получить значение Open для указанной свечи (цена открытия свечи)
--    local H = t[i].high; -- Получить значение High для указанной свечи (наибольшая цена свечи)
--    local L = t[i].low; -- Получить значение Low для указанной свечи (наименьшая цена свечи)
--    local C = t[i].close; -- Получить значение Close для указанной свечи (цена закрытия свечи)
--    local V = t[i].volume; -- Получить значение Volume для указанной свечи (объем сделок в свече)
  for i = n - coveredCandles, n - 1 do
  	openIO:write(tableCandle[i].open 	 .. "\n")
  	highIO:write(tableCandle[i].high 	 .. "\n")
  	lowIO:write(tableCandle[i].low 		 .. "\n")
  	closeIO:write(tableCandle[i].close 	 .. "\n")
  	volumeIO:write(tableCandle[i].volume .. "\n")
  end

  --f:write(data)

  openIO:close()
  highIO:close()
  lowIO:close()
  closeIO:close()
  volumeIO:close()
end