export default function FormField({ type, attr, label, className, defaultValue, disabled}) {
    let css = "block w-full rounded-md border-1 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 ";
    if (className) {
        css += className;
    }

    return (
        <div>
            <div className="flex items-center justify-between">
            <label htmlFor={attr} className="block text-sm font-medium leading-6 text-gray-900">
                {label}
            </label>
            </div>
            <div className="mt-2">
            <input
                id={attr}
                name={attr}
                type={type}
                autoComplete={attr}
                defaultValue={defaultValue ? defaultValue : ""}
                disabled={disabled}
                required
                className={css}
            />
            </div>
        </div>
    );
}