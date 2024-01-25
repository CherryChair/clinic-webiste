export default function ButtonCancel({type, text, onClick, className}) {
    let css = "flex w-full justify-center rounded-md bg-gray-500 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-gray-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gray-500 ";
    if (className) {
        css += className;
    }
    return (
        <button
            type={type}
            onClick={onClick}
            className={css}
        >
        {text}
        </button>
    );
}