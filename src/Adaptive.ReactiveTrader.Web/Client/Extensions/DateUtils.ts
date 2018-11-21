class DateUtils {
    public static formatDateDayMonth(date: Date): string {
        // example date: Fri Apr 18 2014 01:00:00 GMT+0100 (GMT Summer Time)
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1];
    }   
    
    public static formatDateDayMonthYear(date: Date): string {
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1] + " " + tokens[3].substr(2, 2);
    }     
    
    public static formatDateDayMonthYearHour(date: Date): string {
        var tokens = date.toString().split(" ");
        return tokens[2] + " " + tokens[1] + " " + tokens[3].substr(2, 2) + " " + tokens[4].substr(0, 5);
    }       
} 