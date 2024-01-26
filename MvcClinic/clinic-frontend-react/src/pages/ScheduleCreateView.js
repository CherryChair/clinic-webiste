import { useEffect, useState } from "react";
import axios from "axios";
import ErrorBox from "../components/ErrorBox";
import ButtonAccept from "../components/ButtonAccept";
import FormField from "../components/FormField";
import SuccessBox from "../components/SuccessBox";
import EmployeesFormField from "../components/EmployeesFormField";


export default function ScheduleCreatePage() {
    const [schedule, setSchedule] = useState(Object());
    const [errorFlag, setErrorFlag] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    const [successFlag, setSuccessFlag] = useState(false);
    const [successMsg, setSuccessMsg] = useState("");

    useEffect(() => {

    }, []);

    const setError = (msg) => {
        setErrorMsg(msg);
        setErrorFlag(true);
    };

    const clearErrorFlag = () => {
        setErrorMsg("");
        setErrorFlag(false);
    };

    function setSuccess(msg) {
        setSuccessFlag(true);
        setSuccessMsg(msg);
    };

    function clearSuccessFlag() {
        setSuccessFlag(false);
        setSuccessMsg("");
    }
    
    const handleSubmit = event => {
        clearErrorFlag();
        clearSuccessFlag();
        event.preventDefault();
        let formData = event.target;
        let schedulePayload = {
            id: schedule.id,
            firstName: formData.firstName.value,
            surname: formData.surname.value,
            email: formData.email.value,
            specialityId: formData.specialityId.value === "" ? -1 : formData.specialityId.value,
            concurrencyStamp: schedule.concurrencyStamp,
        }
        axios.post("/Schedules/create", schedulePayload).then(response => {
            let tempEmp = schedule;
            tempEmp.concurrencyStamp = response.data;
            setSchedule(tempEmp);
            clearErrorFlag();
            setSuccess("Schedule updated");
        }).catch(err => {
            console.log(err);
            clearSuccessFlag();
            if (err.response.status === 409) {
                setError("Schedule data was changed");
            } else {
                setError("Not found");
            }
        });
    };

    return (
        <>
        <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
          <div className="sm:mx-auto sm:w-full sm:max-w-sm">
            <h2 className="mt-10 text-left text-2xl font-bold leading-9 tracking-tight text-gray-900">
               Create schedule
            </h2>
          </div>
  
          <div className="mt-5 sm:mx-auto sm:w-full sm:max-w-sm">
            <SuccessBox successFlag={successFlag} changeSuccessFlag={clearSuccessFlag} successMsg={successMsg}/>
            <ErrorBox errorFlag={errorFlag} changeErrorFlag={clearErrorFlag} errorMsg={errorMsg}/>
            <form className="space-y-6" onSubmit={handleSubmit}>
                <FormField type="datetime-local" attr="dateFrom" label="Date From"/>
                <FormField type="datetime-local" attr="dateTo" label="Date To"/>
                <EmployeesFormField/>
                <div>
                    <ButtonAccept type="submit" text="Save"/>
                </div>
            </form>
          </div>
        </div>
      </>
    );
}